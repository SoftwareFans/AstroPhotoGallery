using AstroPhotoGallery.Web.Migrations;
using Microsoft.Owin;
using Owin;
using System.Data.Entity;
using AstroPhotoGallery.Data;

[assembly: OwinStartupAttribute(typeof(AstroPhotoGallery.Web.Startup))]
namespace AstroPhotoGallery.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<GalleryDbContext, Configuration>());

            ConfigureAuth(app);
        }
    }
}
