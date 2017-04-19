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
                    .Include(p => p.PicUploader)
                    .Include(p => p.Tags)
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
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .Include(p => p.Tags)
                    .FirstOrDefault();

                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                return View(picture);
            }
        }

        // Method for getting the next picture from a category
        public ActionResult NextPicture(int? categoryId, int? picId)
        {
            if (categoryId == null || picId == null)
            {
                this.AddNotification("No picture or category ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Getting the next pictures' IDs after the current one from the DB
                var nextPicsIDs = db.Pictures
                    .Where(p => p.CategoryId == categoryId)
                    .Select(p => p.Id)
                    .Where(p => p > picId)
                    .ToList();

                var nextPicId = nextPicsIDs.FirstOrDefault();

                var picture = db.Pictures
                    .Where(p => (p.Id == nextPicId) && (p.CategoryId == categoryId))
                    .Include(p => p.PicUploader)
                    .Include(p => p.Tags)
                    .FirstOrDefault();

                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                return RedirectToAction("Details", new { id = picture.Id });
            }
        }

        // Method for getting the previous picture from a category
        public ActionResult PreviousPicture(int? categoryId, int? picId)
        {
            if (categoryId == null || picId == null)
            {
                this.AddNotification("No picture or category ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                // Getting the previous pictures' IDs before the current one from the DB:
                var previousPicsIDs = db.Pictures
                    .Where(p => p.CategoryId == categoryId)
                    .Select(p => p.Id)
                    .Where(p => p < picId)
                    .ToList();

                previousPicsIDs.Reverse();
                var previousPicId = previousPicsIDs.FirstOrDefault();

                var picture = db.Pictures
                    .Where(p => (p.Id == previousPicId) && (p.CategoryId == categoryId))
                    .Include(p => p.PicUploader)
                    .Include(p => p.Tags)
                    .FirstOrDefault();

                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
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
                        var picFileName = Path.GetFileName(poImgFile.FileName);

                        var path = Path.Combine(Server.MapPath("~/Content/images/astroPics"), picFileName);

                        // In case the picture already exists notification is shown:
                        if (System.IO.File.Exists(path))
                        {
                            this.AddNotification("Picture with this name of the file already exists.", NotificationType.ERROR);

                            model.Categories = db.Categories
                           .OrderBy(c => c.Name)
                           .ToList();

                            return View(model);
                        }

                        poImgFile.SaveAs(path);

                        if (ImageValidator.IsImageValid(path))
                        {
                            picture.ImagePath = "~/Content/images/astroPics/" + picFileName;

                            // Getting the name of the category to add it to the current picture's property:
                            var picCategoryName = db.Categories
                                .Where(c => c.Id == picture.CategoryId)
                                .Select(c => c.Name)
                                .ToArray();

                            picture.CategoryName = picCategoryName[0];

                            AdjustCategoryPositions(db, picture, "Upload");

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

        // Method for adjusting the positional boolean variables of a pic required for the "Next" and "Previous" buttons and methods
        private static void AdjustCategoryPositions(GalleryDbContext db, Picture picture, string operation)
        {
            switch (operation)
            {
                case "Upload":

                    // Get all IDs of the pics in the category of the pic being uploaded
                    var allPicsIdsInCategory = db.Pictures
                        .Where(p => p.CategoryId == picture.CategoryId)
                        .Select(p => p.Id)
                        .ToList();

                    db.Pictures.Add(picture);
                    db.SaveChanges();

                    // In case there are already pics in that category before the upload
                    if (allPicsIdsInCategory.Count != 0)
                    {
                        var first = allPicsIdsInCategory.Min(); // The first pic in the category
                        var last = allPicsIdsInCategory.Max(); // The last pic in the category

                        // If there is only 1 pic in the category before the upload
                        if (first == last)
                        {
                            var onlyPicInThatCategory = db.Pictures
                                .Where(p => p.Id == first)
                                .FirstOrDefault();

                            // The only picture is not last anymore because the newly uploaded one will have higher ID
                            onlyPicInThatCategory.IsLastOfCategory = false;

                            db.Entry(onlyPicInThatCategory).State = EntityState.Modified;

                            // The uploaded picture will have higher ID than the only picture before the upload so it will be last of category
                            picture.IsFirstOfCategory = false;
                            picture.IsLastOfCategory = true;
                        }
                        // If there are 2 or more pics in the category before the upload
                        else
                        {
                            // If the uploaded pic's ID is lower than than the first one before the upload
                            if (picture.Id < first)
                            {
                                picture.IsFirstOfCategory = true;

                                // Modifying the first pic in a category before the upload not to be first anymore
                                var previousMinPic = db.Pictures
                                    .Where(p => p.Id == first)
                                    .FirstOrDefault();

                                previousMinPic.IsFirstOfCategory = false;

                                db.Entry(previousMinPic).State = EntityState.Modified;

                            }
                            // If the uploaded pic's ID is higher than than the last one before the upload
                            else if (picture.Id > last)
                            {
                                picture.IsLastOfCategory = true;

                                // Modifying the last pic in a category before the upload not to be last anymore
                                var previousMaxPic = db.Pictures
                                    .Where(p => p.Id == last)
                                    .FirstOrDefault();

                                previousMaxPic.IsLastOfCategory = false;

                                db.Entry(previousMaxPic).State = EntityState.Modified;
                            }
                        }
                    }
                    // In case there are no pics in the category before the currently uploaded one
                    else
                    {
                        picture.IsFirstOfCategory = true;
                        picture.IsLastOfCategory = true;
                    }

                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                    break;

                case "Edit":

                    // Get all IDs of the pics in the category of the pic being edited
                    allPicsIdsInCategory = db.Pictures
                        .Where(p => p.CategoryId == picture.CategoryId)
                        .Select(p => p.Id)
                        .ToList();

                    // Remove the currently edited picture from the collection
                    allPicsIdsInCategory.Remove(picture.Id);

                    // If there are any pics other than the currently edited
                    if (allPicsIdsInCategory.Count > 0)
                    {
                        var first = allPicsIdsInCategory.Min();   // The first pic in the category
                        var last = allPicsIdsInCategory.Max();   // The last pic in the category

                        // If there is only 1 pic in the category before the edit
                        if (first == last)
                        {
                            var onlyPicInThatCategory = db.Pictures
                                .Where(p => p.Id == first)
                                .FirstOrDefault();

                            // If the only picture's ID is lower than the edited one's then the edited one is now last in the category
                            if (onlyPicInThatCategory.Id < picture.Id)
                            {
                                onlyPicInThatCategory.IsFirstOfCategory = true;
                                onlyPicInThatCategory.IsLastOfCategory = false;

                                picture.IsFirstOfCategory = false;
                                picture.IsLastOfCategory = true;
                            }
                            // If the currently edited picture's ID is lower than the only picture then the edited one is now first in the category
                            else
                            {
                                onlyPicInThatCategory.IsFirstOfCategory = false;
                                onlyPicInThatCategory.IsLastOfCategory = true;

                                picture.IsFirstOfCategory = true;
                                picture.IsLastOfCategory = false;
                            }
                            db.Entry(onlyPicInThatCategory).State = EntityState.Modified;
                            db.Entry(picture).State = EntityState.Modified;
                        }
                        // If there are 2 or more pictures in the category before the edit
                        else
                        {
                            // If the edited pic's ID is lower than the first pic in the category then the edited pic becomes first
                            if (picture.Id < first)
                            {
                                picture.IsFirstOfCategory = true;
                                picture.IsLastOfCategory = false;

                                // Modifying the first pic in a category before the edit not to be first anymore
                                var previousMinPic = db.Pictures
                                    .Where(p => p.Id == first)
                                    .FirstOrDefault();

                                previousMinPic.IsFirstOfCategory = false;
                                previousMinPic.IsLastOfCategory = false;

                                db.Entry(previousMinPic).State = EntityState.Modified;

                            }
                            // If the edited pic's ID is higher than the last pic in the category then the edited pic becomes last
                            else if (picture.Id > last)
                            {
                                picture.IsLastOfCategory = true;
                                picture.IsFirstOfCategory = false;

                                // Modifying the last pic in a category before the edit not to be last anymore
                                var previousMaxPic = db.Pictures
                                    .Where(p => p.Id == last)
                                    .FirstOrDefault();

                                previousMaxPic.IsFirstOfCategory = false;
                                previousMaxPic.IsLastOfCategory = false;

                                db.Entry(previousMaxPic).State = EntityState.Modified;
                            }
                            // If the edited picture is between the first and the last then it is not first or last
                            else
                            {
                                picture.IsLastOfCategory = false;
                                picture.IsFirstOfCategory = false;
                            }
                        }
                    }
                    // In case there are no pics in the category except the edited one then the edited one is first and last
                    else
                    {
                        picture.IsFirstOfCategory = true;
                        picture.IsLastOfCategory = true;
                    }

                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                    break;

                case "Delete":

                    // Get the ID of the category of the pic that is being deleted
                    var previousCategoryIdOfPic = picture.CategoryId;

                    // Get the IDs of the pics from the category of the pic that is being deleted
                    var previousCategoryPicsIdToBeChanged = db.Pictures
                           .Where(p => p.CategoryId == previousCategoryIdOfPic)
                           .Select(p => p.Id)
                           .ToList();

                    // Remove the currently deleted pic from the collection
                    previousCategoryPicsIdToBeChanged.Remove(picture.Id);

                    // If there are any other pics in the category other than the one being deleted
                    if (previousCategoryPicsIdToBeChanged.Count > 0)
                    {
                        var first = previousCategoryPicsIdToBeChanged.Min();   // The first pic in the category
                        var last = previousCategoryPicsIdToBeChanged.Max();   // The last pic in the category

                        // If there is only 1 pic in the category before the delete except the one being deleted then it becomes now first and last
                        if (first == last)
                        {
                            var onlyPicture = db.Pictures
                                .Where(p => p.Id == first)
                                .FirstOrDefault();

                            onlyPicture.IsLastOfCategory = true;
                            onlyPicture.IsFirstOfCategory = true;

                            db.Entry(onlyPicture).State = EntityState.Modified;
                        }
                        // If there are 2 or more pics in the category before the delete except the one being deleted
                        else
                        {
                            // The first pic is marked to be first now
                            var firstPic = db.Pictures.Where(p => p.Id == first).FirstOrDefault();
                            firstPic.IsFirstOfCategory = true;
                            firstPic.IsLastOfCategory = false;
                            db.Entry(firstPic).State = EntityState.Modified;

                            // The last pic is marked to be last now
                            var lastPic = db.Pictures.Where(p => p.Id == last).FirstOrDefault();
                            lastPic.IsFirstOfCategory = false;
                            lastPic.IsLastOfCategory = true;
                            db.Entry(lastPic).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                    }

                    break;
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

                // Check if picture exists
                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                ViewBag.TagsString = string.Join(", ", picture.Tags.Select(t => t.Name));

                if (!IsUserAuthorizedToEditAndDelete(picture))
                {
                    this.AddNotification("You don't have the necessary authority to delete this picture.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                // Pass picture to the view
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

                // Adjust the position boolean variables of the other pics before the deletion
                AdjustCategoryPositions(db, picture, "Delete");

                // Getting the category of the pic before the deletion in order to redirect to Home/ListPictures after that
                var picCategoryId = (int?)picture.CategoryId;

                // Delete the picture from the ~/Content/images/astroPics folder:
                var picPath = picture.ImagePath;
                var mappedPath = Server.MapPath(picPath);
                System.IO.File.Delete(mappedPath);

                // Delete the picture from the database 
                db.Pictures.Remove(picture);
                db.SaveChanges();

                // Redirect to the page with all pics in the current category page
                this.AddNotification("The picture was deleted.", NotificationType.SUCCESS);
                return RedirectToAction("ListPictures", "Home", new { categoryId = picCategoryId });
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

                    if (picture == null)
                    {
                        this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                        return RedirectToAction("ListCategories", "Home");
                    }

                    // Set picture properties
                    picture.PicTitle = model.PicTitle;
                    picture.PicDescription = model.PicDescription;

                    // Getting the category of the pic in case it is being changed
                    var previousCategoryIdOfPic = picture.CategoryId;

                    picture.CategoryId = model.CategoryId;
                    var picCategoryName = db.Categories
                        .Where(c => c.Id == model.CategoryId)
                        .Select(c => c.Name)
                        .ToArray();
                    picture.CategoryName = picCategoryName[0];

                    this.SetPictureTags(picture, model, db);

                    // Save pic state in database
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();

                    // If the category of the pic is changed
                    if (previousCategoryIdOfPic != picture.CategoryId)
                    {
                        // Adjust the position boolean variables of the other pics before the edit
                        AdjustCategoryPositions(db, picture, "Edit");

                        // Get the IDs of the pictures from the previous category of the edited pic
                        var previousCategoryPicsIdToBeChanged = db.Pictures
                            .Where(p => p.CategoryId == previousCategoryIdOfPic)
                            .Select(p => p.Id)
                            .ToList();

                        // If there are pics from the previous category of the edited pic
                        if (previousCategoryPicsIdToBeChanged.Count > 0)
                        {
                            // The first pic in the category
                            var first = previousCategoryPicsIdToBeChanged.Min();

                            // The last pic in the category
                            var last = previousCategoryPicsIdToBeChanged.Max();    

                            // If there is only 1 pic in the category it becomes now first and last one there
                            if (first == last)
                            {
                                var onlyPicture = db.Pictures
                                    .Where(p => p.Id == first)
                                    .FirstOrDefault();

                                onlyPicture.IsLastOfCategory = true;
                                onlyPicture.IsFirstOfCategory = true;

                                db.Entry(onlyPicture).State = EntityState.Modified;
                            }
                            // If there are 2 or more pics in the category before the edit
                            else
                            {
                                var firstPic = db.Pictures.Where(p => p.Id == first).FirstOrDefault();
                                firstPic.IsFirstOfCategory = true;
                                firstPic.IsLastOfCategory = false;
                                db.Entry(firstPic).State = EntityState.Modified;

                                var lastPic = db.Pictures.Where(p => p.Id == last).FirstOrDefault();
                                lastPic.IsFirstOfCategory = false;
                                lastPic.IsLastOfCategory = true;
                                db.Entry(lastPic).State = EntityState.Modified;
                            }

                            db.SaveChanges();
                        }
                    }

                    return View("Details", picture);
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

        public ActionResult DownlandFile(string filePath)
        {
            // The names of the pictures are not stored in DB so the name is taken from the path of the pic
            var filename = filePath.Substring(27);
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