using Autofac;
using Autofac.Integration.Mvc;
using System.Reflection;
using System.Web.Mvc;

namespace AstroPhotoGallery.DependencyResolution
{
    public class DependencyResolver
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

            var container = builder.Build();

            return new AutofacDependencyResolver(container);
        }
    }
}
