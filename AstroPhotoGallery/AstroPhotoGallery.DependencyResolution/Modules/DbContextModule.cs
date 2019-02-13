using AstroPhotoGallery.Data;
using Autofac;

namespace AstroPhotoGallery.DependencyResolution.Modules
{
   public class DbContextModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GalleryDbContext>().AsSelf().InstancePerRequest();
        }
    }
}
