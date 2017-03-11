using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AstroPhotoGallery.Startup))]
namespace AstroPhotoGallery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
