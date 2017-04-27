using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Models;
using AstroPhotoGallery.Extensions;
using Microsoft.AspNet.Identity;
using PagedList;

namespace AstroPhotoGallery.Controllers
{
    public class PictureController : Controller
    {
        //
        // GET: Picture
        public ActionResult Index()
        {
            return RedirectToAction("ListCategories", "Home");
        }

        //
        //GET: Picture/Upload
        [Authorize]
        public ActionResult Upload()
        {
            using (var db = new GalleryDbContext())
            {
                var model = new PictureViewModel
                {
                    Categories = db.Categories
                    .OrderBy(c => c.Name)
                    .ToList()
                };

                return View(model);
            }
        }

        //
        //POST: Picture/Upload
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(PictureViewModel model, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    // Get uploader's ID
                    var uploaderId = db.Users
                        .First(u => u.UserName == User.Identity.Name)
                        .Id;

                    var picture = new Picture(uploaderId, model.PicTitle, model.PicDescription, model.CategoryId);

                    // Getting the name of the category to add it to the current picture's property:
                    var picCategoryName = db.Categories
                        .Where(c => c.Id == picture.CategoryId)
                        .Select(c => c.Name)
                        .ToArray();

                    picture.CategoryName = picCategoryName[0];

                    SetPictureTags(picture, model, db);

                    if (image != null && image.ContentLength > 0)
                    {
                        var imagesOfThatCategoryDir = $"~/Content/images/astroPics/{picture.CategoryName}/";
                        var picFileName = image.FileName;
                        var uploadPath = imagesOfThatCategoryDir + picFileName;
                        var physicalPath = Server.MapPath(uploadPath);

                        // In case the picture already exists a notification is shown:
                        if (System.IO.File.Exists(physicalPath))
                        {
                            this.AddNotification("Picture with this file name already exists in that category.", NotificationType.ERROR);

                            model.Categories = db.Categories
                                .OrderBy(c => c.Name)
                                .ToList();

                            return View(model);
                        }

                        image.SaveAs(physicalPath);

                        if (ImageValidator.IsImageValid(physicalPath))
                        {
                            picture.ImagePath = uploadPath;

                            AdjustCategoryPositions.Upload(db, picture);

                            return RedirectToAction("Details", new { id = picture.Id });
                        }
                        else
                        {
                            // Deleting the file from ~/Content/images/astroPics/:
                            System.IO.File.Delete(physicalPath);

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

        //
        //GET: Picture/Details/id
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

        //
        //GET: Picture/Edit/id
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

                var model = new PictureViewModel
                {
                    Id = picture.Id,
                    PicTitle = picture.PicTitle,
                    PicDescription = picture.PicDescription,
                    CategoryId = picture.CategoryId,
                    ImagePath = picture.ImagePath,

                    Categories = db.Categories
                    .OrderBy(c => c.Name)
                    .ToList(),

                    Tags = string.Join(" ", picture.Tags.Select(t => t.Name))
                };

                return View(model);
            }
        }

        //
        //POST: Picture/Edit
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
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

                    SetPictureTags(picture, model, db);

                    // Save pic state in database
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();

                    // If the category of the pic is changed
                    if (previousCategoryIdOfPic != picture.CategoryId)
                    {
                        // Adjust the positional boolean variables of the other pics before the edit
                        AdjustCategoryPositions.Edit(db, picture);

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
                                    .FirstOrDefault(p => p.Id == first);

                                onlyPicture.IsLastOfCategory = true;
                                onlyPicture.IsFirstOfCategory = true;

                                db.Entry(onlyPicture).State = EntityState.Modified;
                            }
                            // If there are 2 or more pics in the category before the edit
                            else
                            {
                                var firstPic = db.Pictures.FirstOrDefault(p => p.Id == first);
                                firstPic.IsFirstOfCategory = true;
                                firstPic.IsLastOfCategory = false;
                                db.Entry(firstPic).State = EntityState.Modified;

                                var lastPic = db.Pictures.FirstOrDefault(p => p.Id == last);
                                lastPic.IsFirstOfCategory = false;
                                lastPic.IsLastOfCategory = true;
                                db.Entry(lastPic).State = EntityState.Modified;
                            }

                            db.SaveChanges();
                        }

                        // Getting the old directory of the pic
                        var picOldDir = Server.MapPath(picture.ImagePath);

                        var picFileName = picture.ImagePath.Substring(picture.ImagePath.LastIndexOf('/') + 1);

                        picture.ImagePath = $"~/Content/images/astroPics/{picture.CategoryName}/{picFileName}";

                        db.Entry(picture).State = EntityState.Modified;
                        db.SaveChanges();

                        var picNewDir = Server.MapPath(picture.ImagePath);
                        System.IO.File.Move(picOldDir, picNewDir);
                    }
                    return View("Details", picture);
                }
            }

            using (var db = new GalleryDbContext())
            {
                model.Categories = db.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                model.ImagePath = db.Pictures.First(p => p.Id == model.Id).ImagePath;

                return View(model);
            }
        }

        //GET: Picture/Delete/id
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

                ViewBag.TagsString = string.Join(" ", picture.Tags.Select(t => t.Name));

                if (!IsUserAuthorizedToEditAndDelete(picture))
                {
                    this.AddNotification("You don't have the necessary authority to delete this picture.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                // Pass picture to the view
                return View(picture);
            }
        }

        //
        //POST: Picture/Delete/id
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
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

                // Adjust the positional boolean variables of the other pics before the deletion
                AdjustCategoryPositions.Delete(db, picture);

                // Getting the category of the pic before the deletion in order to redirect to Home/ListPictures after that
                var picCategoryId = (int?)picture.CategoryId;

                // Delete the picture from the ~/Content/images/astroPics folder:
                var physicalPath = Server.MapPath(picture.ImagePath);
                System.IO.File.Delete(physicalPath);

                // Delete the picture from the database 
                db.Pictures.Remove(picture);
                db.SaveChanges();

                // Redirect to the page with all pics in the current category
                this.AddNotification("The picture was deleted.", NotificationType.SUCCESS);
                return RedirectToAction("ListPictures", "Home", new { categoryId = picCategoryId });
            }
        }
      
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult RatePicture(FormCollection form)
        {
            var rating = int.Parse(form["Rating"]);
            var pictureId = int.Parse(form["PictureId"]);

            if (rating != 0)
            {
                using (var db = new GalleryDbContext())
                {
                    var picture = db.Pictures.FirstOrDefault(p => p.Id == pictureId);

                    if (picture == null)
                    {
                        this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                        return RedirectToAction("ListCategories", "Home");
                    }

                    picture.RatingSum += rating;
                    picture.RatingCount++;
                    picture.Rating = picture.RatingSum / picture.RatingCount;

                    // Getting the ID of the currently logged in user
                    var currentUserId = User.Identity.GetUserId();

                    // Adding the ID to the IDs of the users that have already rated this picture
                    picture.UserIdsRatedPic += " " + currentUserId;

                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();

                    if (rating == 1)
                    {
                        this.AddNotification("The picture was rated with 1 star.", NotificationType.SUCCESS);
                    }
                    else
                    {
                        this.AddNotification($"The picture was rated with {rating} stars.", NotificationType.SUCCESS);
                    }
                }
            }

            return RedirectToAction("Details", new { id = pictureId });
        }

        //
        //GET: Picture/ShowUserPics/id
        public ActionResult ShowUserPics(string id, int ? page )
        {
            if (id == null)
            {
                this.AddNotification("No user ID provided.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == id);

                if (user == null)
                {
                    this.AddNotification("Such a user doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("ListCategories", "Home");
                }

                var picsOfUser = db.Pictures
                    .Where(p => p.PicUploaderId == id)
                    .OrderByDescending(p => p.Id)
                    .Include(p => p.PicUploader)
                    .Include(p => p.Tags)
                    .ToList();

                ViewBag.UserFullname = user.FirstName + " " + user.LastName;

                int pageSize = 8;
                int pageNumber = (page ?? 1);
                return View(picsOfUser.ToPagedList(pageNumber, pageSize));
            }
        }

        //Set tags of a picture
        private void SetPictureTags(Picture picture, PictureViewModel model, GalleryDbContext db)
        {
            //Split tags
            var tagsStrings = model.Tags
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.ToLower())
                .Distinct();

            //Clear current picture tags
            picture.Tags.Clear();

            //Set new picture tags
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

        //Method for download picture to user PC
        public ActionResult DownlandFile(string filePath)
        {
            var filename = filePath.Substring(filePath.LastIndexOf('/') + 1);
            var fileExtension = Path.GetExtension(filename).ToLower();

            if (fileExtension.Equals(".jpg") ||
                fileExtension.Equals(".png") ||
                fileExtension.Equals(".jpeg") ||
                fileExtension.Equals(".bmp") ||
                fileExtension.Equals(".ico"))
            {
                var dir = Server.MapPath(filePath);

                var type = fileExtension.Substring(1);
                var imageType = $"image/{type}";

                return File(dir, imageType, filename);
            }
            else
            {
                this.AddNotification("Invalid picture format.", NotificationType.ERROR);
                return RedirectToAction("ListCategories", "Home");
            }
        }

        //Check user is authorized
        public bool IsUserAuthorizedToEditAndDelete(Picture picture)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = picture.IsUploader(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}