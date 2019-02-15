using AstroPhotoGallery.Data;
using Autofac;
using Autofac.Integration.Mvc;
using System.Reflection;
using System.Web.Mvc;

namespace AstroPhotoGallery.DependencyResolution
{
    public class AstroPhotoGalleryDependencyResolver
    {
        public static IDependencyResolver WebDependencyResolver()
        {
            // Create your builder.
            var builder = new ContainerBuilder();

            var web = Assembly.GetCallingAssembly();
            // Register your MVC controllers. 
            builder.RegisterControllers(web);

            builder.RegisterType<ExtensibleActionInvoker>().As<IActionInvoker>();

            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            //TODO:
            //builder.RegisterType<GalleryDbContext>().AsSelf();
            //builder.RegisterType<GalleryDbContext>().As<DbContext>().InstancePerRequest();
            var container = builder.Build();

            return new AutofacDependencyResolver(container);
        }
    }
}
