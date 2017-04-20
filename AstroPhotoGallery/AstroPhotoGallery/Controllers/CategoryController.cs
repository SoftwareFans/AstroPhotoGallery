using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AstroPhotoGallery.Extensions;
using AstroPhotoGallery.Models;
using PagedList;

namespace AstroPhotoGallery.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        //
        //GET: Category
        public ActionResult Index(string sortOrder, string seatchCategory, string currentFilter, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Name = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : string.Empty;

            if (seatchCategory != null)
            {
                page = 1;
            }
            else
            {
                seatchCategory = currentFilter;
            }

            ViewBag.CurrentFilter = seatchCategory;

            using (var db = new GalleryDbContext())
            {

                var categories = db.Categories.ToList();

                if (!string.IsNullOrEmpty(seatchCategory))
                {
                    categories = categories.Where(c => c.Name.ToLower().Contains(seatchCategory.ToLower())).ToList();
                }

                switch (sortOrder)
                {
                    case "Name_desc":
                        categories = categories.OrderByDescending(s => s.Name).ToList();
                        break;
                }

                int pageSize = 10;
                int pageNumber = (page ?? 1);

                return View(categories.ToPagedList(pageNumber, pageSize));
            }
        }

        //
        //GET: Category/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        //POST: Category/Create
        [HttpPost]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                using (var db = new GalleryDbContext())
                {
                    if (db.Categories.Any(c => c.Name == category.Name))
                    {
                        this.AddNotification("Category with this name already exists.", NotificationType.ERROR);

                        return RedirectToAction("Index");
                    }

                    db.Categories.Add(category);
                    db.SaveChanges();
                    this.AddNotification("Category created.", NotificationType.SUCCESS);

                    return RedirectToAction("Index");
                }
            }

            return View(category);
        }

        //
        //GET: Category/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No category ID provided.", NotificationType.ERROR);
                return RedirectToAction("Index");
            }

            using (var db = new GalleryDbContext())
            {
                var category = db.Categories.FirstOrDefault(c => c.Id == id);

                if (category == null)
                {
                    this.AddNotification("Such a category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
                }

                return View(category);
            }
        }

        //
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

                    // When the name of a category is being changed all the pictures in that category in DB should be changed:
                    var picsToBeChanged = db.Pictures.Where(p => p.CategoryId == category.Id).ToList();
                    foreach (var pic in picsToBeChanged)
                    {
                        pic.CategoryName = category.Name;
                        db.Entry(pic).State = EntityState.Modified;
                    }

                    db.SaveChanges();

                    this.AddNotification("Category edited.", NotificationType.SUCCESS);

                    return RedirectToAction("Index");
                }
            }

            return View(category);
        }

        //
        //GET: Category/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No category ID provided.", NotificationType.ERROR);
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

        //
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
                    // Delete the pic from ~/Content/images:
                    string path = pic.ImagePath;
                    var mappedPath = Server.MapPath(path);
                    System.IO.File.Delete(mappedPath);

                    // Delete the pic from DB
                    db.Pictures.Remove(pic);
                }

                db.Categories.Remove(category);
                db.SaveChanges();
                this.AddNotification("Category deleted.", NotificationType.SUCCESS);

                return RedirectToAction("Index");
            }
        }
    }
}