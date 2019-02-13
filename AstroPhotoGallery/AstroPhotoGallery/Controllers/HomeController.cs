using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AstroPhotoGallery.Data;
using AstroPhotoGallery.Web.Extensions;
using AstroPhotoGallery.Services.Interfaces;
using PagedList;

namespace AstroPhotoGallery.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITestService _test;

        public HomeController(ITestService test)
        {
            this._test = test;
        }

        //
        //GET: Home/Index
        public ActionResult Index()
        {
            var test = this._test.GetData();

            using (var db = new GalleryDbContext())
            {
                //Get pictures from database
                var categories = db.Categories
                   .Include(c => c.Pictures)
                   .OrderByDescending(c => c.Id)
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
                    .OrderByDescending(c => c.Id)
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
                        .OrderByDescending(c => c.Id)
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
        public ActionResult ListPictures(int? id, int? page)
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

                TempData["CategoryName"] = category.Name;
             
                var pictures = db.Pictures
                    .Where(p => p.CategoryId == id)
                    .OrderBy(p => p.Id)
                    .Include(p => p.PicUploader)
                    .Include(p => p.Tags)
                    .ToList();

                int pageSize = 8;
                int pageNumber = (page ?? 1);
                return View(pictures.ToPagedList(pageNumber, pageSize));              
            }
        }

        //
        //GET: Home/Contacts
        public ActionResult Contacts()
        {
            return View();
        }

        //
        //GET: Home/About
        public ActionResult About()
        {
            return View();
        }
    }
}