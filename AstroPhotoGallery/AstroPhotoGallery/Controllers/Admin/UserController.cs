using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AstroPhotoGallery.Controllers.Admin
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //GET: User/List
        public ActionResult List()
        {
            using (var db = new GalleryDbContext())
            {
                var users = db.Users
                    .ToList();

                var admins = GetAdminUserNames(users, db);
                ViewBag.Admins = admins;

                return View(users);
            }
        }

        private HashSet<string> GetAdminUserNames(List<ApplicationUser> users, GalleryDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var admins =new HashSet<string>();

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