using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Models;

namespace AstroPhotoGallery.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new GalleryDbContext())
            {
                //Get pictures from database

                var categories = db.Categories
                   .Include(c => c.Pictures)
                   .OrderBy(c => c.Name)
                   .Take(3)
                   .ToList();

                return View(categories);
            }
        }

        public ActionResult ListCategories()
        {
            using (var db = new GalleryDbContext())
            {
                var categories = db.Categories
                    .Include(c => c.Pictures)
                    .OrderBy(c => c.Name)
                    .ToList();

                return View(categories);
            }
        }

        public ActionResult ListPictures( int ? categoryId)
        {
            if (categoryId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new GalleryDbContext())
            {
                var pictures = db.Pictures
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.PicUploader)
                    .ToList();

                return View(pictures);
            }
        }
    }
}
