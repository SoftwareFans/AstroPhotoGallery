using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AstroPhotoGallery.Extensions;
using AstroPhotoGallery.Models;

namespace AstroPhotoGallery.Controllers
{
    [Authorize(Roles = "Admin")]
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

        //GET: Category/Create
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
                using (var db = new GalleryDbContext())
                {
                    if(db.Categories.Any(c=>c.Name == category.Name))
                    {
                        this.AddNotification("Category already exists.", NotificationType.ERROR);
                        return RedirectToAction("Index");
                    }

                    db.Categories.Add(category);
                    db.SaveChanges();
                    this.AddNotification("Category created.",NotificationType.SUCCESS);                   
                    return RedirectToAction("Index");
                }
            }

            return View(category);
        }

        //GET: Category/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No category ID provided.", NotificationType.ERROR);
                return RedirectToAction("List");
            }

            using (var db = new GalleryDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                return View(category);
            }
        }

        //POST: Category/Edit
        [HttpPost]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    if (db.Categories.Any(c => c.Name == category.Name))
                    {
                        this.AddNotification("Category already exists.", NotificationType.ERROR);
                        return RedirectToAction("List");
                    }

                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                    this.AddNotification("Category edited.", NotificationType.SUCCESS);
                    return RedirectToAction("List");
                }
            }
            return View(category);
        }

        //GET: Category/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No category ID provided.", NotificationType.ERROR);
                return RedirectToAction("List");
            }

            using (var db = new GalleryDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                return View(category);
            }
        }

        //POST: Category/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            using (var db = new GalleryDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("List");
                }

                var categoryPictures = category.Pictures.ToList();

                foreach (var pic in categoryPictures)
                {
                    db.Pictures.Remove(pic);
                }

                db.Categories.Remove(category);
                db.SaveChanges();
                this.AddNotification("Category deleted.", NotificationType.SUCCESS);
                return RedirectToAction("List");
            }
        }       
    }
}