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

        public async Task<IQueryable<Category>> GetGategoriesAsync()
        {
            var categories = _dbContext.Categories.Select(x=>x); //Make IQueryable

            return await Task.FromResult(categories);
        }
    }
}
