using System.Linq;
using System.Web.Mvc;
using AstroPhotoGallery.Web.Extensions;
using AstroPhotoGallery.Models;
using PagedList;
using static AstroPhotoGallery.Common.Globals;
using AstroPhotoGallery.Services.Interfaces.Admin;
using System.Threading.Tasks;
using AstroPhotoGallery.Web.Models.Category;
using System;
using AutoMapper;
using AstroPhotoGallery.Common.Resources;
using AstroPhotoGallery.Web.Helpers;

namespace AstroPhotoGallery.Web.Controllers.Admin
{
    [Authorize(Roles = UserRoleDef.Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        private const string OrderNameDesc = "Name_desc";
        private static string MainDirectoryPath = "~/Content/images/astroPics/"; //todo: in common if necessary
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
                    this.AddNotification(NotificationMessagesRecources.msgNoSearchingCategoryExist, NotificationType.INFO);
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
                this.SetNotificationForUnExistingCategory();
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
                    await this.AddCategory(category);
                }
                else
                {
                    await this.EditCategory(category);
                }

                await this._categoryService.SaveCategoryAsync(category, isAdded);
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }          

        [HttpGet]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await this._categoryService.GetCategoryByIdAsync(id);

            if (category == null)
            {
                this.SetNotificationForUnExistingCategory();
                return RedirectToAction(nameof(Index));
            }

            var viewModel = Mapper.Map<Category, DeleteCategoryViewModel>(category);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(DeleteCategoryViewModel viewModel)
        {
            var category = await this._categoryService.GetCategoryByIdAsync(viewModel.Id);

            if (category == null)
            {
                this.SetNotificationForUnExistingCategory();
                return RedirectToAction(nameof(Index));
            }

            await this._categoryService.RemoveCategoryWithPicsAsync(category);
            DeleteCategoryAndPicsFromServer(category.Name);

            this.AddNotification(NotificationMessagesRecources.msgDeleteCategory, NotificationType.SUCCESS);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Add new category
        /// </summary>
        private async Task AddCategory(Category category)
        {
            await Task.Run(() => this.CreateCategoryDirectory(category.Name));
            this.AddNotification(NotificationMessagesRecources.msgCreatedCategory, NotificationType.SUCCESS);
        }

        /// <summary>
        /// Edit existing category
        // When the name of a category is being changed 
        // all the pictures in that category in DB must be changed
        /// </summary>
        private async Task EditCategory(Category category)
        {
            await UpdateCategoryDirecoryAsync(category.Id, category.Name);

            await this._categoryService.UpdateAndSavePicturesFromCategoryAsync(category.Id, category.Name);
            this.AddNotification(NotificationMessagesRecources.msgEditedCategory, NotificationType.SUCCESS);
        }

        /// <summary>
        /// Custom validation of model, check if category exist in db
        /// </summary>
        /// <param name="viewModel">model to be validate</param>
        private async Task CustomModelValidateAddEditCategory(AddEditCategoryViewModel viewModel)
        {
            var categoryName = viewModel.Name;
            var categoryExist = await this._categoryService.CategoryAlreadyExistsAsync(categoryName);

            if (categoryExist)
            {
                ModelState.AddModelError(nameof(viewModel.Name), ValidationModelMessageRecources.msgCategorAlreadyExist);
            }
        }

        /// <summary>
        /// Creating the folder of the category on the server
        /// </summary>
        private void CreateCategoryDirectory(string categoryName)
        {
            var categoryDir = Server.MapPath($"{MainDirectoryPath}{categoryName}");
            DirectoryHelper.CreateDirectory(categoryDir);
        }

        /// <summary>
        /// Rename(move) the category's directory + all pics in it
        /// Trim name because if has blank space string throw exception
        /// </summary>
        private async Task UpdateCategoryDirecoryAsync(int categoryId, string categoryName)
        {
            var categoryOldName = await this._categoryService.GetCategoryNameAsync(categoryId);

            var trimEnd = categoryName.TrimEnd();
            var trimStart = trimEnd.TrimStart();

            var catOldDir = Server.MapPath($"{MainDirectoryPath}{categoryOldName}/");
            var catNewDir = Server.MapPath($"{MainDirectoryPath}{trimStart}/");

            DirectoryHelper.RenameDirectory(catOldDir, catNewDir);
        }

        /// <summary>
        /// Delete the category's directory + all files in it(recursive true)
        /// </summary>
        private void DeleteCategoryAndPicsFromServer(string categoryName)
        {
            var directoryTyDelete = Server.MapPath($"{MainDirectoryPath}{categoryName}");
            DirectoryHelper.RemoveDirectory(directoryTyDelete);
        }

        /// <summary>
        /// Add notification message for un existing category
        /// </summary>
        private void SetNotificationForUnExistingCategory()
        {
            this.AddNotification(NotificationMessagesRecources.msgUnExistingCategory, NotificationType.ERROR);
        }
    }
}