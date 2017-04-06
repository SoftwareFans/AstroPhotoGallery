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
                    this.AddNotification("Category created.",NotificationType.INFO);                   
                    return RedirectToAction("Index");
                }
            }

            this.AddNotification("Please enter name for the category.", NotificationType.ERROR);
            return View(category);
        }

        //GET: Category/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                return RedirectToAction("Index");
            }

            using (var db = new GalleryDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
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
                        return RedirectToAction("Index");
                    }

                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();
                    this.AddNotification("Category edited.", NotificationType.INFO);
                    return RedirectToAction("Index");
                   
                }
            }

            this.AddNotification("Please enter name for the category.", NotificationType.ERROR);
            return View(category);
        }

        //GET: Category/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                return RedirectToAction("Index");
            }

            using (var db = new GalleryDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    this.AddNotification("Category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
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
                    return RedirectToAction("Index");
                }

                var categoryPictures = category.Pictures.ToList();

                foreach (var pic in categoryPictures)
                {
                    db.Pictures.Remove(pic);
                }

                db.Categories.Remove(category);
                db.SaveChanges();

                this.AddNotification("Category deleted.", NotificationType.INFO);
                return RedirectToAction("Index");
            }
        }       
    }
}