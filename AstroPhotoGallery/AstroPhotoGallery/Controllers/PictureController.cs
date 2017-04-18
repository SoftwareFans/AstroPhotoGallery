using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using AstroPhotoGallery.Models;
using AstroPhotoGallery.Extensions;

namespace AstroPhotoGallery.Controllers
{
    public class PictureController : Controller
    {
        // GET: Picture
        public ActionResult Index()
        {
            return RedirectToAction("ListCategories", "Home");
        }

        //GET: Picture/List - THIS CAN BE REMOVED BECAUSE IT IS NOT USED CURRENTLY
        public ActionResult List()
        {
            using (var db = new GalleryDbContext())
            {
                // Get pictures from database
                var pictures = db.Pictures
                    .Include(x => x.PicUploader)
                    .Include(x=>x.Tags)
                    .ToList();

                return View(pictures);
            }
        }

        //GET: Picture/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No picture ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Get picture from database
                var picture = db.Pictures
                    .Where(x => x.Id == id)
                    .Include(x => x.PicUploader)
                    .Include(x=>x.Tags)
                    .FirstOrDefault();

                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                var picsOfCategory = db.Pictures
                    .Where(p => p.CategoryId == picture.CategoryId)
                    .ToList();

                // If there is only 1 picture in a category the boolean variable is changed for proper work of the "NextPicture" and "PreviousPicture" actions:
                if (picsOfCategory.Count == 1)
                {
                    picture.IsFirstOfCategory = true;
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return View(picture);
            }
        }

        public ActionResult NextPicture(int? categoryId, int? picId)
        {
            if (categoryId == null || picId == null)
            {
                this.AddNotification("No picture or category ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Getting the next picture after the current one from the DB
                var nextPicsIDs = db.Pictures.Select(p => p.Id).Where(p => p > picId).ToList();
                var nextPic = nextPicsIDs.FirstOrDefault();
                var picture = db.Pictures.Where(x => (x.Id == nextPic) && (x.CategoryId == categoryId)).Include(u => u.PicUploader).FirstOrDefault();

                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                // In case the next picture is the last picture in that category we set the boolean variable to true and that way hiding the "NextPicture" button of the view
                if (nextPicsIDs.Count == 1)
                {
                    picture.IsLastOfCategory = true;
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("Details", new { id = picture.Id });
            }
        }

        public ActionResult PreviousPicture(int? categoryId, int? picId)
        {
            if (categoryId == null || picId == null)
            {
                this.AddNotification("No picture or category ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Getting the previous picture before the current from the DB:
                var previousPicsIDs = db.Pictures.Select(p => p.Id).Where(p => p < picId).ToList();
                previousPicsIDs.Reverse();
                var previousPic = previousPicsIDs.FirstOrDefault();

                var picture = db.Pictures.Where(x => (x.Id == previousPic) && (x.CategoryId == categoryId)).Include(u => u.PicUploader).FirstOrDefault();

                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                // In case the previous picture is the first picture in that category we set the boolean variable to true and that way hiding the "PreviousPicture" button of the view
                if (previousPicsIDs.Count == 1)
                {
                    picture.IsFirstOfCategory = true;
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("Details", new { id = picture.Id });
            }
        }

        //GET: Picture/Upload
        [Authorize]
        public ActionResult Upload()
        {
            using (var db = new GalleryDbContext())
            {
                var model = new PictureViewModel();
                model.Categories = db.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(model);
            }
        }

        //POST: Picture/Upload
        [HttpPost]
        [Authorize]
        public ActionResult Upload([Bind(Exclude = "ImagePath")]PictureViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    // Get uploader's id
                    var uploaderId = db.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                    var picture = new Picture();
                    picture.PicUploaderId = uploaderId;
                    picture.PicTitle = model.PicTitle;
                    picture.PicDescription = model.PicDescription;
                    picture.CategoryId = model.CategoryId;
                    this.SetPictureTags(picture, model, db);

                    if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                    {
                        var poImgFile = Request.Files["ImagePath"];
                        var pic = Path.GetFileName(poImgFile.FileName);

                        var path = Path.Combine(Server.MapPath("~/Content/images/astroPics"), pic);

                        // In case the picture already exists notification is shown:
                        if (System.IO.File.Exists(path))
                        {
                            this.AddNotification("Picture with this name already exists.", NotificationType.ERROR);

                            model.Categories = db.Categories
                           .OrderBy(c => c.Name)
                           .ToList();

                            return View(model);
                        }

                        poImgFile.SaveAs(path);

                        if (ImageValidator.IsImageValid(path))
                        {
                            picture.ImagePath = "~/Content/images/astroPics/" + pic;

                            // Getting the name of the category to add it to the current picture's property:
                            var picCategoryName = db.Categories.Where(c => c.Id == picture.CategoryId).Select(c => c.Name).ToArray();
                            picture.CategoryName = picCategoryName[0];

                            // Every new uploaded picture is last in its category
                            picture.IsLastOfCategory = true;
                            db.Pictures.Add(picture);
                            db.SaveChanges();

                            // Getting the ID of the previous picture in DB and if there is such we change it not to be the last in its category
                            var previousPicsIDs = db.Pictures
                                .Select(p => p.Id)
                                .Where(p => p < picture.Id)
                                .ToList();

                            previousPicsIDs.Reverse();
                            var lastPicId = previousPicsIDs.FirstOrDefault();
                            var previousPic = db.Pictures
                                .Where(p => (p.Id == lastPicId) && (p.CategoryId == picture.CategoryId))
                                .FirstOrDefault();

                            if (previousPic != null)
                            {
                                previousPic.IsLastOfCategory = false;
                                db.Entry(previousPic).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            return RedirectToAction("Details", new { id = picture.Id });
                        }
                        else
                        {
                            // Deleting the file from ~/Content/images/astroPics:
                            System.IO.File.Delete(path);

                            this.AddNotification("Invalid picture format.", NotificationType.ERROR);

                            model.Categories = db.Categories
                           .OrderBy(c => c.Name)
                           .ToList();

                            return View(model);
                        }
                    }
                    else
                    {
                        this.AddNotification("No picture selected for upload or empty file chosen.", NotificationType.ERROR);

                        model.Categories = db.Categories
                            .OrderBy(c => c.Name)
                            .ToList();

                        return View(model);
                    }
                }
            }
            using (var db = new GalleryDbContext())
            {
                model.Categories = db.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(model);
            }
        }

        //GET: Picture/Delete
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No picture ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .Include(p => p.Category)
                    .FirstOrDefault();

                ViewBag.TagsString = string.Join(", ", picture.Tags.Select(t => t.Name));

                // Check if picture exists
                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                if (!IsUserAuthorizedToEditAndDelete(picture))
                {
                    this.AddNotification("You don't have the necessary authority to delete this picture.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                // Pass picture to view
                return View(picture);
            }
        }

        //POST: Picture/Delete
        [HttpPost]
        [ActionName("Delete")]
        [Authorize]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No picture ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .Include(p => p.Category)
                    .FirstOrDefault();

                // Check if picture exists
                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                // Delete the picture from the database 
                var picPath = picture.ImagePath;
                db.Pictures.Remove(picture);
                db.SaveChanges();

                // Delete the picture from the ~/Content/images/astroPics folder:
                var mappedPath = Server.MapPath(picPath);
                System.IO.File.Delete(mappedPath);

                // Redirect to index page
                this.AddNotification("The picture was deleted.", NotificationType.SUCCESS);
                return RedirectToAction("ListCategories", "Home");
            }
        }

        //GET: Picture/Edit
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No picture ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .FirstOrDefault();

                // Check if picture exists
                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                if (!IsUserAuthorizedToEditAndDelete(picture))
                {
                    this.AddNotification("You don't have the necessary authority to edit this picture.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                var model = new PictureViewModel();
                model.Id = picture.Id;
                model.PicTitle = picture.PicTitle;
                model.PicDescription = picture.PicDescription;
                model.CategoryId = picture.CategoryId;
                model.ImagePath = picture.ImagePath;
                model.Categories = db.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                model.Tags = string.Join(", ", picture.Tags.Select(t => t.Name));

                return View(model);
            }
        }

        //POST: Picture/Edit
        [Authorize]
        [HttpPost]
        public ActionResult Edit(PictureViewModel model)
        {
            // Check if model state is valid
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    // Get picture form database
                    var picture = db.Pictures
                        .Include(u => u.PicUploader)
                        .FirstOrDefault(p => p.Id == model.Id);

                    // Set picture properties
                    picture.PicTitle = model.PicTitle;
                    picture.PicDescription = model.PicDescription;
                    picture.CategoryId = model.CategoryId;
                    var picCategoryName = db.Categories.Where(c => c.Id == model.CategoryId).Select(c => c.Name).ToArray();
                    picture.CategoryName = picCategoryName[0];
                    this.SetPictureTags(picture, model, db);
                    // Save pic state in database
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                    return View("Details", picture);
                }
            }

            using (var db = new GalleryDbContext())
            {
                model.Categories = db.Categories.OrderBy(c => c.Name).ToList();

                return View(model);
            }
        }

        public ActionResult DownlandFile(string filePath)
        {
            // The names of the pictures are not stored in DB so the name is taken from the path of the pic
            var filename = filePath.Substring(17);
            var fileExtension = Path.GetExtension(filename);

            if (fileExtension.Equals(".jpg") ||
                fileExtension.Equals(".png") ||
                fileExtension.Equals(".jpeg") ||
                fileExtension.Equals(".bmp") ||
                fileExtension.Equals(".ico"))
            {
                var dir = Server.MapPath("~/Content/images/astroPics");
                var path = Path.Combine(dir, filename); //validate the path for security or use other means to generate the path.

                var type = fileExtension.Substring(1);
                var imageType = $"image/{type}";

                return base.File(path, imageType, filename);
            }
            else
            {
                this.AddNotification("Invalid picture format.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }
        }

        private void SetPictureTags(Picture picture, PictureViewModel model, GalleryDbContext db)
        {
            //Split tags
            var tagsStrings = model.Tags
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .Distinct();

            //Clear current picture tags
            picture.Tags.Clear();

            //Set new pictre tags

            foreach (var tagString in tagsStrings)
            {
                //Get tag from db by its name
                var tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));

                //If the tag is null create new tag
                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    db.Tags.Add(tag);
                }

                //Add tag to picture tags
                picture.Tags.Add(tag);
            }
        }

        public bool IsUserAuthorizedToEditAndDelete(Picture picture)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = picture.IsUploader(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}