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
        public ActionResult Upload()
        {
            return View();
        }

        //POST: Picture/Upload
        [HttpPost]
        public ActionResult Upload([Bind(Exclude = "ImagePath")]Picture picture)
        {
            if (ModelState.IsValid)
            {

                //if (Request.Files.Count > 0)
                //{
                //    var poImgFile = Request.Files["ImagePath"].FileName;
                //    var pic = Path.GetFileName(poImgFile.FileName);
                //    path = Path.Combine(Server.MapPath("~/Content/images"), pic);
                //    poImgFile.SaveAs(path);
                //}

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
                        var path = $"~/Content/images/{Request.Files["ImagePath"].FileName}";
                        picture.ImagePath = path;
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

            using (var db =new GalleryDbContext())
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
    }
}