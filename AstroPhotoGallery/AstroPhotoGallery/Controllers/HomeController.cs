using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AstroPhotoGallery.Extensions;
using AstroPhotoGallery.Models;
using PagedList;

namespace AstroPhotoGallery.Controllers
{
    public class HomeController : Controller
    {
        //
        //GET: Home/Index
        public ActionResult Index()
        {
            using (var db = new GalleryDbContext())
            {
                //Get pictures from database
                var categories = db.Categories
                   .Include(c => c.Pictures)
                   .OrderBy(c => c.Name)
                   .Take(6)
                   .ToList();

                return View(categories);
            }
        }

        //
        //GET: Home/ListCategories
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
                    this.AddNotification("No categories found.", NotificationType.ERROR);

                    return RedirectToAction("Index");
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    categories = categories
                        .Where(c => c.Name.ToLower()
                        .Contains(searchString.ToLower()))
                        .ToList();

                    if (categories.Count == 0)
                    {
                        this.AddNotification("No categories containing this string were found.", NotificationType.INFO);                       
                    }
                }

                int pageSize = 6;
                int pageNumber = (page ?? 1);

                return View(categories.ToPagedList(pageNumber, pageSize));
            }
        }

        //
        //GET: Home/ListPictures/id
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
                    this.AddNotification("Such a category doesn't exist.", NotificationType.ERROR);
                    return RedirectToAction("Index");
                }

                TempData["CategoryName"] = category.Name;

                var pictures = db.Pictures
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.PicUploader)
                    .Include(p => p.Tags)
                    .ToList();

                return View(pictures);
            }
        }

        public ActionResult Contacts()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}