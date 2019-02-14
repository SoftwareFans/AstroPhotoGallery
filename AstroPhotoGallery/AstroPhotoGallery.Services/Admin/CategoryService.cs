using System.Data.Entity;
using AstroPhotoGallery.Data;
using AstroPhotoGallery.Models;
using AstroPhotoGallery.Services.Interfaces.Admin;
using System.Linq;
using System.Threading.Tasks;

namespace AstroPhotoGallery.Services.Admin
{
    public class CategoryService : ICategoryService
    {
        private readonly GalleryDbContext _dbContext;

        public CategoryService(GalleryDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<bool> CategoryAlreadyExists(string categoryName)
        {
            var allCategories = await this.GetGategoriesAsync();

            var exists = allCategories.Any(x => x.Name == categoryName);

            return exists;
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var categories = await this.GetGategoriesAsync();

            var result = categories.FirstOrDefault(x => x.Id == id);

            return result;
        }

        public async Task<IQueryable<Category>> GetGategoriesAsync()
        {
            var categories = _dbContext.Categories.Select(x => x);

            return await Task.FromResult(categories);
        }

        public async Task SaveCategory(Category category, bool isAdded)
        {
            this._dbContext.Entry(category).State = EntityState.Detached;
            this._dbContext.Entry(category).State = isAdded ? EntityState.Added : EntityState.Modified;

            //only save modified props if entity is not added
            await this._dbContext.SaveChangesAsync();
        }

        public async Task<string> GetCategoryNameAsync(int categoryId)
        {
            var category = await this._dbContext.Categories.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == categoryId);

            var categoryName = category?.Name;

            return categoryName;
        }

        public async Task UpdateAndSavePicturesFromCategoryAsync(int categoryId, string categoryName)
        {
            var picsToBeChanged = this._dbContext.Pictures
                .Include(x=>x.Category)
                .Where(p => p.CategoryId == categoryId);

            foreach (var pic in picsToBeChanged)
            {
                var picFileName = pic.ImagePath.Substring(pic.ImagePath.LastIndexOf('/') + 1);
                pic.ImagePath = $"~/Content/images/astroPics/{categoryName}/{picFileName}";

                //TODO: ??? 
                //database.Entry(pic).State = EntityState.Modified;
            }

          await  this._dbContext.SaveChangesAsync();
        }
    }
}
