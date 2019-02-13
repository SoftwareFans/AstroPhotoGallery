using AstroPhotoGallery.Services.Interfaces;

namespace AstroPhotoGallery.Services
{
    public class TestService : ITestService
    {
        public string GetData()
        {
            return "Test";
        }
    }
}
