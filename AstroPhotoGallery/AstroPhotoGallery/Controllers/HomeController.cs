using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Extensions;
using AstroPhotoGallery.Models;
using PagedList;

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
                   .Take(4)
                   .ToList();

                return View(categories);
            }
        }

        public ActionResult ListCategories(string searchString, int? page, string currentFilter)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            using (var db = new GalleryDbContext())
            {
                var categories = db.Categories
                    .Include(c => c.Pictures)
                    .OrderBy(c => c.Name)
                    .ToList();

                if (categories.Count == 0)
                {
                    categories.Add(new Category { Name = "defimages", });
                }
                if (!String.IsNullOrEmpty(searchString))
                {
                    categories = categories.Where(s => s.Name.Contains(searchString)).ToList();
                }

                int pageSize = 8;
                int pageNumber = (page ?? 1);
                return View(categories.ToPagedList(pageNumber, pageSize));
            }

        }

        public ActionResult ListPictures(int? categoryId)
        {
            if (categoryId == null)
            {
                this.AddNotification("No category ID provided.", NotificationType.ERROR);
                return RedirectToAction("Index");
            }

            using (var db = new GalleryDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == categoryId);
                if (category == null)
                {
                    this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
                }

                var pictures = db.Pictures
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.PicUploader)
                    .ToList();

                return View(pictures);
            }
        }
    }
}
