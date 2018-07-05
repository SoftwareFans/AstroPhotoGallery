using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Data;
using AstroPhotoGallery.Extensions;
using AstroPhotoGallery.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using PagedList;

namespace AstroPhotoGallery.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        //
        // GET: User
        public ActionResult Index(string sortOrder, string searchUser, string currentFilter, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Email = string.IsNullOrEmpty(sortOrder) ? "Email_desc" : String.Empty;

            if (searchUser != null)
            {
                page = 1;
            }
            else
            {
                searchUser = currentFilter;
            }

            ViewBag.CurrentFilter = searchUser;

            using (var db = new GalleryDbContext())
            {
                var users = db.Users
                    .OrderBy(u => u.Email)
                    .ToList();

                if (!string.IsNullOrEmpty(searchUser))
                {
                    users = users.Where(u => u.Email.ToLower().Contains(searchUser.ToLower())).ToList();
                    if (users.Count == 0)
                    {
                        this.AddNotification("No users' emails containing this string were found.", NotificationType.INFO);
                    }
                }

                var admins = this.GetAdminUserNames(users, db);

                ViewBag.Admins = admins;

                switch (sortOrder)
                {
                    case "Email_desc":
                        users = users.OrderByDescending(u => u.Email).ToList();
                        break;
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);

                return View(users.ToPagedList(pageNumber, pageSize));
            }
        }

        //
        //GET: User/Edit/id
        public ActionResult Edit(string id)
        {
            //Validate id
            if (id == null)
            {
                this.AddNotification("No user ID provided.", NotificationType.ERROR);
                return RedirectToAction("Index");
            }

            using (var db = new GalleryDbContext())
            {
                //Get user from database
                var user = db.Users
                    .FirstOrDefault(u => u.Id == id);

                //Check if user exists
                if (user == null)
                {
                    this.AddNotification("Such a user doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
                }

                //Create a view model
                var model = new EditUserViewModel
                {
                    User = user,
                    Roles = this.GetUserRoles(user, db)
                };

                //Pass the model to the view
                return View(model);
            }
        }

        //
        //POST: User/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, EditUserViewModel viewModel)
        {
            //Check if model is valid
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    //Get user from database 
                    var user = db
                        .Users
                        .FirstOrDefault(u => u.Id == id);

                    //Check if user exists
                    if (user == null)
                    {
                        this.AddNotification("Such a user doesn't exist.", NotificationType.ERROR);
                        return RedirectToAction("Index");
                    }

                    //If password field is not empty, change password
                    if (!string.IsNullOrEmpty(viewModel.Password))
                    {
                        var hasher = new PasswordHasher();
                        var passwordHash = hasher.HashPassword(viewModel.Password);
                        user.PasswordHash = passwordHash;
                    }

                    //Set user properties
                    user.Email = viewModel.User.Email;
                    user.FirstName = viewModel.User.FirstName;
                    user.LastName = viewModel.User.LastName;
                    user.UserName = viewModel.User.Email;
                    SetUserRoles(viewModel, user, db);

                    //Save changes
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();

                    this.AddNotification("The user was edited.", NotificationType.SUCCESS);
                    return RedirectToAction("Index");
                }
            }

            return View(viewModel);
        }

        //
        //GET: User/Delete/id
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                this.AddNotification("No user ID provided.", NotificationType.ERROR);
                return RedirectToAction("Index");
            }

            using (var db = new GalleryDbContext())
            {
                //Get user from database
                var user = db.Users
                    .FirstOrDefault(u => u.Id == id);

                //Check if user exists
                if (user == null)
                {
                    this.AddNotification("Such a user doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
                }

                return View(user);
            }
        }

        //
        //POST: User/Delete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == null)
            {
                this.AddNotification("No user ID provided.", NotificationType.ERROR);
                return RedirectToAction("Index");
            }

            using (var db = new GalleryDbContext())
            {
                //Get user from database
                var user = db.Users
                    .FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    this.AddNotification("Such a user doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
                }

                //Get user's pictures form database
                var userPictures = db.Pictures
                    .Where(p => p.PicUploader.Id == user.Id)
                    .ToList();

                //Delete user's pictures
                foreach (var picture in userPictures)
                {
                    //Adjust the positional boolean variables of the other pics before the deletion
                    AdjustCategoryPositions.Delete(db, picture);
                    
                    //Delete the picture from the server
                    var physicalPath = Server.MapPath(picture.ImagePath);
                    System.IO.File.Delete(physicalPath);

                    db.Pictures.Remove(picture);
                    db.SaveChanges();
                }

                // Delete the user's directory and the pic in there from ~/Content/images/profilePics:
                var userIdFolder = user.Id;
                var userDirectory = Server.MapPath($"~/Content/images/profilePics/{userIdFolder}");
                if (Directory.Exists(userDirectory))
                {
                    Directory.Delete(userDirectory, true);
                }

                //Delete user and save changes 
                db.Users.Remove(user);
                db.SaveChanges();

                this.AddNotification("The user was deleted.", NotificationType.SUCCESS);
                return RedirectToAction("Index");
            }
        }

        private void SetUserRoles(EditUserViewModel model, ApplicationUser user, GalleryDbContext db)
        {
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.IsSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }

        private IList<Role> GetUserRoles(ApplicationUser user, GalleryDbContext db)
        {
            //Create user manager 
            var userManager = Request
                .GetOwinContext()
                .GetUserManager<ApplicationUserManager>();

            //Get all application roles
            var roles = db.Roles
                .Select(r => r.Name)
                .OrderBy(r => r)
                .ToList();

            //For each application role, check if the user has it
            var userRoles = new List<Role>();

            foreach (var roleName in roles)
            {
                var role = new Role { Name = roleName };

                if (userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }

                userRoles.Add(role);
            }

            //Return a list with all roles
            return userRoles;
        }

        private HashSet<string> GetAdminUserNames(List<ApplicationUser> users, GalleryDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var admins = new HashSet<string>();

            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.UserName);
                }
            }

            return admins;
        }
    }
}