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
            var categories = _dbContext.Categories.Select(x=>x); 

            return await Task.FromResult(categories);
        }

        public async Task SaveCategory(Category category)
        {
            this._dbContext.Categories.Add(category);
            await this._dbContext.SaveChangesAsync();
        }
    }
}
