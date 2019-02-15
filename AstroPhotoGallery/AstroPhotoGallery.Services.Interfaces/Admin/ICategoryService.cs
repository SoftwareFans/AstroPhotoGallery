using AstroPhotoGallery.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AstroPhotoGallery.Services.Interfaces.Admin
{
    public interface ICategoryService
    {
        Task<IQueryable<Category>> GetGategoriesAsync();

        Task<Category> GetCategoryByIdAsync(int id);

        Task<bool> CategoryAlreadyExists(string categoryName);

        Task SaveCategory(Category category, bool isAdded);

        Task<string> GetCategoryNameAsync(int categoryId);

        Task UpdateAndSavePicturesFromCategoryAsync(int categoryId, string categoryName);

        Task RemoveCategoryWithPicsAsync(Category category);
    }
}
