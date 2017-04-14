using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
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
            return RedirectToAction("List");
        }

        //GET: Picture/List
        public ActionResult List()
        {
            using (var bd = new GalleryDbContext())
            {
                //Get pictures from database

                var pictures = bd.Pictures
                    .Include(x => x.PicUploader)
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
                return RedirectToAction("List");
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from database

                var picture = db.Pictures.Where(x => x.Id == id).Include(u => u.PicUploader).FirstOrDefault();

                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                return View(picture);
            }
        }

        //GET: Picture/Upload
        [Authorize]
        public ActionResult Upload()
        {
            using (var db = new GalleryDbContext())
            {
                var model = new Picture();
                model.Categories = db.Categories.OrderBy(c => c.Name).ToList();

                return View(model);
            }
        }

        //POST: Picture/Upload
        [HttpPost]
        [Authorize]
        public ActionResult Upload([Bind(Exclude = "ImagePath")]Picture picture)
        {
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    //Get uploader id
                    var uploaderId = db.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                    //Set picture uploader
                    picture.PicUploaderId = uploaderId;
                    picture.Categories = db.Categories.OrderBy(c => c.Name).ToList();

                    if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                    {
                        var poImgFile = Request.Files["ImagePath"];
                        var pic = Path.GetFileName(poImgFile.FileName);

                        var path = Path.Combine(Server.MapPath("~/Content/images"), pic);
                        poImgFile.SaveAs(path);

                        if (IsValidImage(path))
                        {
                            picture.ImagePath = "~/Content/images/" + pic;

                            // Getting the name of the category to add it to the current picture's property:
                            var picCategoryName = db.Categories.Where(c => c.Id == picture.CategoryId).Select(c => c.Name).ToArray();
                            picture.CategoryName = picCategoryName[0];

                            //Save pic in database
                            db.Pictures.Add(picture);
                            db.SaveChanges();

                            return RedirectToAction("ListCategories", "Home");
                        }
                        else
                        {
                            // Deleting the file from ~/Content/images:
                            System.IO.File.Delete(path);

                            this.AddNotification("Invalid picture format.", NotificationType.ERROR);
                            return View(picture);
                        }
                    }
                    else
                    {
                        this.AddNotification("No picture selected for upload", NotificationType.ERROR);
                        return View(picture);
                    }
                }
            }
            using (var db = new GalleryDbContext())
            {
                picture.Categories = db.Categories.OrderBy(c => c.Name).ToList();
            }

            return View(picture);
        }

        //GET: Picture/Delete
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No picture ID provided.", NotificationType.ERROR);
                return RedirectToAction("List");
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .FirstOrDefault();

                //Check if picture exists
                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                if (!IsUserAuthorizedToEditAndDelete(picture))
                {
                    this.AddNotification("You don't have the necessary authority to delete this picture.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                //Pass picture to view
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
                return RedirectToAction("List");
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .FirstOrDefault();

                //Check if picture exists
                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                //Delete the picture from the database 
                var picPath = picture.ImagePath;
                db.Pictures.Remove(picture);
                db.SaveChanges();

                //Delete the picture from the ~/Content/images folder:
                var mappedPath = Server.MapPath(picPath);
                System.IO.File.Delete(mappedPath);

                //Redirect to index page
                return RedirectToAction("List");
            }
        }

        //GET: Picture/Edit
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No picture ID provided.", NotificationType.ERROR);
                return RedirectToAction("List");
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .FirstOrDefault();

                //Check if picture exists
                if (picture == null)
                {
                    this.AddNotification("Such a picture doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                if (!IsUserAuthorizedToEditAndDelete(picture))
                {
                    this.AddNotification("You don't have the necessary authority to edit this picture.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                picture.Categories = db.Categories.OrderBy(c => c.Name).ToList();
                return View(picture);
            }
        }

        //POST: Picture/Edit
        [Authorize]
        [HttpPost]
        public ActionResult Edit(Picture model)
        {
            //Check if model state is valid
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    //Get picture form database
                    var picture = db.Pictures.Include(u => u.PicUploader).FirstOrDefault(p => p.Id == model.Id);

                    //Set picture properties
                    picture.PicTitle = model.PicTitle;
                    picture.PicDescription = model.PicDescription;
                    picture.CategoryId = model.CategoryId;
                    var picCategoryName = db.Categories.Where(c => c.Id == model.CategoryId).Select(c => c.Name).ToArray();
                    picture.CategoryName = picCategoryName[0];

                    //Save pic state in database
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();
                    return View("Details", picture);
                }
            }

            using (var db = new GalleryDbContext())
            {
                var picture = db.Pictures.FirstOrDefault(p => p.Id == model.Id);
                picture.Categories = db.Categories.OrderBy(c => c.Name).ToList();
                return View(picture);
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
                var dir = Server.MapPath("~/Content/images");
                var path = Path.Combine(dir, filename); //validate the path for security or use other means to generate the path.

                var type = fileExtension.Substring(1);
                var imageType = $"image/{type}";

                return base.File(path, imageType, filename);
            }           
            else
            {
                this.AddNotification("Invalid picture format.", NotificationType.ERROR);
                return RedirectToAction("List");
            }
        }

        // Primary method for checking if the image is in valid format:
        public static bool IsValidImage(string filePath)
        {
            return System.IO.File.Exists(filePath) &&
                   IsValidStream(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        // Method for IsValidImage - checking the data passed to the stream:
        public static bool IsValidStream(FileStream inputStream)
        {
            using (inputStream)
            {
                Dictionary<string, List<byte[]>> imageHeaders = new Dictionary<string, List<byte[]>>();

                // For every image type there is a list of byte arrays with its headers possible values.
                // Other image types can be added in case of need - TIF, GIF etc.

                imageHeaders.Add("JPG", new List<byte[]>());
                imageHeaders.Add("JPEG", new List<byte[]>());
                imageHeaders.Add("PNG", new List<byte[]>());
                imageHeaders.Add("BMP", new List<byte[]>());
                imageHeaders.Add("ICO", new List<byte[]>());

                imageHeaders["JPG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
                imageHeaders["JPG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 });
                imageHeaders["JPG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xFE });
                imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
                imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 });
                imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xFE });
                imageHeaders["PNG"].Add(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
                imageHeaders["BMP"].Add(new byte[] { 0x42, 0x4D });
                imageHeaders["ICO"].Add(new byte[] { 0x00, 0x00, 0x01, 0x00 });

                if (inputStream.Length > 0) // If the file has any data at all
                {
                    string fileExt = inputStream.Name.Substring(inputStream.Name.LastIndexOf('.') + 1).ToUpper();

                    if (!(fileExt.Equals("JPG") ||
                        fileExt.Equals("JPEG") ||
                        fileExt.Equals("PNG") ||
                        fileExt.Equals("BMP") ||
                        fileExt.Equals("ICO")
                        ))
                    {
                        return false;
                    }

                    foreach (var subType in imageHeaders[fileExt])
                    {
                        byte[] standardHeader = subType;
                        byte[] checkedHeader = new byte[standardHeader.Length];

                        inputStream.Read(checkedHeader, 0, checkedHeader.Length);

                        if (CompareArrays(standardHeader, checkedHeader))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                    throw new Exception("Empty file chosen.");
                }
            }
        }

        // Method for IsValidImage - comparing two byte arrays:
        public static bool CompareArrays(byte[] firstArr, byte[] secondArr)
        {
            if (firstArr.Length != secondArr.Length)
            {
                return false;
            }

            for (int i = 0; i < firstArr.Length; i++)
            {
                if (firstArr[i] != secondArr[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsUserAuthorizedToEditAndDelete(Picture picture)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = picture.IsUploader(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}