using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using AstroPhotoGallery.Data;
using AstroPhotoGallery.Web.Extensions;
using AstroPhotoGallery.Models;
using PagedList;
using static AstroPhotoGallery.Common.Globals;
using AstroPhotoGallery.Services.Interfaces.Admin;
using System.Threading.Tasks;
using AstroPhotoGallery.Web.Models.Category;
using System;
using AutoMapper;

namespace AstroPhotoGallery.Web.Controllers.Admin
{
    [Authorize(Roles = UserRoleDef.Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        private const string OrderNameDesc = "Name_desc";
        private const int PageSize = 10;
        private const int DefaultPageStartNumber = 1;

        public CategoryController(ICategoryService categoryService)
        {
            this._categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string sortOrder, string searchedCategory, string currentFilter, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Name = string.IsNullOrEmpty(sortOrder) ? OrderNameDesc : string.Empty;

            if (searchedCategory != null)
            {
                page = 1;
            }
            else
            {
                searchedCategory = currentFilter;
            }

            ViewBag.CurrentFilter = searchedCategory;

            var categories = await this._categoryService.GetGategoriesAsync();

            if (!string.IsNullOrEmpty(searchedCategory))
            {
                var searchedCategoryToLower = searchedCategory.ToLower();
                categories = categories.Where(c => c.Name.ToLower().Contains(searchedCategoryToLower));

                if (!categories.Any())
                {
                    //TODO: resources
                    this.AddNotification("No categories containing this string were found..", NotificationType.INFO);
                }
            }

            //Sorting categories by name prop
            categories = sortOrder == OrderNameDesc ?
                categories.OrderByDescending(s => s.Name) :
                categories.OrderBy(s => s.Name);

            var pageSize = PageSize;
            var pageNumber = (page ?? DefaultPageStartNumber);

            return View(categories.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult Add()
        {
            var viewModel = new AddEditCategoryViewModel();
            viewModel.RequestType = RequestType.Add;

            return View(nameof(Edit), viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var category = await this._categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                //TODO: resources
                this.AddNotification("Such a category doesn't exist.", NotificationType.ERROR);
                return RedirectToAction(nameof(Index));
            }

            var viewModel = Mapper.Map<Category, AddEditCategoryViewModel>(category);
            viewModel.RequestType = RequestType.Edit;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(AddEditCategoryViewModel viewModel)
        {
            await this.CustomModelValidateAddEditCategory(viewModel);
            if (ModelState.IsValid)
            {
                var category = Mapper.Map<AddEditCategoryViewModel, Category>(viewModel);
                var isAdded = viewModel.RequestType == RequestType.Add;

                if (isAdded)
                {
                    this.CreateCategoryDirectory(category.Name);
                    this.AddNotification("Category created.", NotificationType.SUCCESS);
                }
                else
                {
                    await UpdateCategoryDirecory(category.Id, category.Name);

                    // When the name of a category is being changed 
                    //all the pictures in that category in DB must be changed
                    await this._categoryService.UpdateAndSavePicturesFromCategoryAsync(category.Id, category.Name);               
                    this.AddNotification("Category edited.", NotificationType.SUCCESS);
                }

                await this._categoryService.SaveCategory(category, isAdded);

                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
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

        private async Task CustomModelValidateAddEditCategory(AddEditCategoryViewModel viewModel)
        {
            var categoryName = viewModel.Name;
            bool categoryExist = await this._categoryService.CategoryAlreadyExists(categoryName);

            if (categoryExist)
            {
                ModelState.AddModelError(nameof(viewModel.Name), "Category with this name already exists");
            }
        }

        private void CreateCategoryDirectory(string categoryName)
        {
            // Creating the folder of the category on the server
            var categoryDir = Server.MapPath($"~/Content/images/astroPics/{categoryName}");
            Directory.CreateDirectory(categoryDir);
        }

        private async Task UpdateCategoryDirecory(int categoryId, string categoryName)
        {
            var categoryOldName = await this._categoryService.GetCategoryNameAsync(categoryId);

            var catOldDir = Server.MapPath($"/Content/images/astroPics/{categoryOldName}/");
            var catNewDir = Server.MapPath($"/Content/images/astroPics/{categoryName}/");
            // Rename(move) the category's directory + all pics in it
            Directory.Move(catOldDir, catNewDir);
        }
    }
}