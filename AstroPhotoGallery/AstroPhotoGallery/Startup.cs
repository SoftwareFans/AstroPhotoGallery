using AstroPhotoGallery.Migrations;
using AstroPhotoGallery.Models;
using Microsoft.Owin;
using Owin;
using System.Data.Entity;

[assembly: OwinStartupAttribute(typeof(AstroPhotoGallery.Startup))]
namespace AstroPhotoGallery
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
