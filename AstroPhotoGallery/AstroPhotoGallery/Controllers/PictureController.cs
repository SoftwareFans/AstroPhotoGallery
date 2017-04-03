using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Models;

namespace AstroPhotoGallery.Controllers
{
    public class PictureController : Controller
    {

        private bool IsUserAuthorizedToEdit(Picture picture)
        {
            //chech for admin
            bool isUploader = picture.IsUploader(this.User.Identity.Name);
            return isUploader;
        }
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from batabase

                var picture = db.Pictures.Where(x => x.Id == id).Include(u => u.PicUploader).First();

                if (picture == null)
                {
                    return HttpNotFound();
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
                    //Pic validation

                    if (Request.Files.Count > 0)
                    {

                        var poImgFile = Request.Files["ImagePath"];
                        var pic = Path.GetFileName(poImgFile.FileName);
                        var path = Path.Combine(Server.MapPath("~/Content/images"), pic);

                        poImgFile.SaveAs(path);
                        picture.ImagePath = "~/Content/images/" + pic; 
                       
                    }

                    //Save pic in database
                    db.Pictures.Add(picture);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            return View(picture);
        }

        //GET: Picture/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .First();              

                if (!IsUserAuthorizedToEdit(picture))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                //Check if picture exists
                if (picture == null)
                {
                    return HttpNotFound();
                }
                //Pass picture to  view

                return View(picture);
            }
        }

        //POST: Picture/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .Include(p => p.PicUploader)
                    .First();

                //Check if picture exists
                if (picture == null)
                {
                    return HttpNotFound();
                }

                //Delete article from database 
                db.Pictures.Remove(picture);
                db.SaveChanges();

                //Redirect to index page
                return RedirectToAction("Index");
            }
        }

        //GET: Picture/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new GalleryDbContext())
            {
                //Get picture from database
                var picture = db.Pictures
                    .Where(p => p.Id == id)
                    .First();

                if (!IsUserAuthorizedToEdit(picture))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                //Chech if picture exists
                if (picture == null)
                {
                    return HttpNotFound();
                }

                //Create view model
                var model = new Picture();
                model.Id = picture.Id;
                model.PicTitle = picture.PicTitle;
                model.PicDescription = picture.PicDescription;
                model.ImagePath = picture.ImagePath;
                model.CategoryId = picture.CategoryId;
                model.Categories = db.Categories.OrderBy(c => c.Name).ToList();

                //Pass the view model to view
                return View(model);
            }
        }

        //POST: Picture/Edit
        [HttpPost]
        public ActionResult Edit(Picture model)
        {
            //Check if model state is valid
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    //Get picture form database
                    var picture = db.Pictures.FirstOrDefault(p => p.Id == model.Id);

                    //Set picture props
                    picture.PicTitle = model.PicTitle;
                    picture.PicDescription = model.PicDescription;
                    picture.CategoryId = model.CategoryId;

                    //Save pic state in database
                    db.Entry(picture).State = EntityState.Modified;
                    db.SaveChanges();

                    //Redirect to the index page
                    return RedirectToAction("Index");
                }
            }

            //If model state is invalid,return the same view
            return View(model);
        }


    }
}