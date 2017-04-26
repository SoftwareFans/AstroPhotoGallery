using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AstroPhotoGallery.Extensions;
using AstroPhotoGallery.Models;
using PagedList;

namespace AstroPhotoGallery.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        //
        //GET: Category
        public ActionResult Index(string sortOrder, string searchedCategory, string currentFilter, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Name = string.IsNullOrEmpty(sortOrder) ? "Name_desc" : string.Empty;

            if (searchedCategory != null)
            {
                page = 1;
            }
            else
            {
                searchedCategory = currentFilter;
            }

            ViewBag.CurrentFilter = searchedCategory;

            using (var db = new GalleryDbContext())
            {

                var categories = db.Categories.ToList();

                if (!string.IsNullOrEmpty(searchedCategory))
                {
                    categories = categories.Where(c => c.Name.ToLower().Contains(searchedCategory.ToLower())).ToList();

                    if (categories.Count == 0)
                    {
                        this.AddNotification("No categories containing this string were found..", NotificationType.INFO);
                    }
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
        [ValidateAntiForgeryToken]
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

                    // Creating the folder of the category on the server
                    var categoryDir = Server.MapPath($"~/Content/images/astroPics/{category.Name}");
                    Directory.CreateDirectory(categoryDir);

                    this.AddNotification("Category created.", NotificationType.SUCCESS);

                    return RedirectToAction("Index");
                }
            }

            return View(category);
        }

        //
        //GET: Category/Edit/id
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
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

                    // Getting the old name of the category
                    var categoryFromDb = db.Categories.FirstOrDefault(c => c.Id == category.Id);
                    if (categoryFromDb == null)
                    {
                        this.AddNotification("Such a category doesn't exist.", NotificationType.ERROR);
                        return RedirectToAction("Index");
                    }

                    var categoryOldName = categoryFromDb.Name;
                    db.Dispose(); // Dispose required in order to use "category" again

                    using (var database = new GalleryDbContext())
                    {
                        database.Entry(category).State = EntityState.Modified;

                        var catOldDir = Server.MapPath($"/Content/images/astroPics/{categoryOldName}/");
                        var catNewDir = Server.MapPath($"/Content/images/astroPics/{category.Name}/");
                        // Rename(move) the category's directory + all pics in it
                        Directory.Move(catOldDir, catNewDir);

                        // When the name of a category is being changed all the pictures in that category in DB must be changed:
                        var picsToBeChanged = database.Pictures
                            .Where(p => p.CategoryId == category.Id)
                            .ToList();

                        foreach (var pic in picsToBeChanged)
                        {
                            pic.CategoryName = category.Name;
                            var picFileName = pic.ImagePath.Substring(pic.ImagePath.LastIndexOf('/') + 1);
                            pic.ImagePath = $"~/Content/images/astroPics/{category.Name}/{picFileName}";

                            database.Entry(pic).State = EntityState.Modified;
                        }

                        database.SaveChanges();

                        this.AddNotification("Category edited.", NotificationType.SUCCESS);

                        return RedirectToAction("Index");
                    }
                }
            }
            return View(category);
        }

        //
        //GET: Category/Delete/id
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
                    this.AddNotification("Such a category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
                }

                return View(category);
            }
        }

        //
        //POST: Category/Delete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
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

                // When a category is being deleted all the pictures in that category in DB must be deleted:
                var picsToBeDeleted = category.Pictures.ToList();

                foreach (var pic in picsToBeDeleted)
                {
                    // Delete the pic from DB
                    db.Pictures.Remove(pic);
                }

                // Delete the category's directory + all files in it(recursive true)
                Directory.Delete(Server.MapPath($"~/Content/images/astroPics/{category.Name}"), true);

                db.Categories.Remove(category);
                db.SaveChanges();

                this.AddNotification("Category deleted.", NotificationType.SUCCESS);

                return RedirectToAction("Index");
            }
        }
    }
}