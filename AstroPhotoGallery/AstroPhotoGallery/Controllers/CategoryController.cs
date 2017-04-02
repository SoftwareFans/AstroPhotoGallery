using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Models;

namespace AstroPhotoGallery.Controllers
{
    public class CategoryController : Controller
    {
        // GET: Category
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //GET: Category/List
        public ActionResult List()
        {
            using (var db = new GalleryDbContext())
            {
                var categories = db.Categories.ToList();

                return View(categories);
            }
        }

        //GET: Categori/Create
        public ActionResult Create()
        {
            return View();
        }

        //POST: Category/Create
        [HttpPost]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                //If modelstate is valid add category to categories in database and redirect to view with all categories
                using (var db = new GalleryDbContext())
                {
                    db.Categories.Add(category);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            //If modelstate is not valid render the same view with category
            return View(category);
        }
    }
}