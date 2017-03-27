using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Models;

namespace AstroPhotoGallery.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
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
    }
}