using System.Linq;
using AstroPhotoGallery.Services.Interfaces;
using AstroPhotoGallery.Data;

namespace AstroPhotoGallery.Services
{
    public class TestService : ITestService
    {
        private readonly GalleryDbContext _dbContext;

        public TestService(GalleryDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public string GetData()
        {
            var test = this._dbContext.Pictures.FirstOrDefault();

            var name = test?.CategoryName;

            return name;
        }
    }
}
