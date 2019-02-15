using AstroPhotoGallery.DependencyResolution;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AstroPhotoGallery.Web.Utils.MapperConfiguration;

namespace AstroPhotoGallery.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyResolver.SetResolver(AstroPhotoGalleryDependencyResolver.WebDependencyResolver());
            AutoMapperConfiguration.Configure();
        }
    }
}
