using AstroPhotoGallery.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AstroPhotoGallery.Services.Interfaces.Admin
{
    public interface ICategoryService
    {
        Task<IQueryable<Category>> GetGategoriesAsync();
    }
}
