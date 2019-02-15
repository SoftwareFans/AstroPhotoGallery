using AstroPhotoGallery.Web.Utils.MapperConfiguration.Maps;
using AutoMapper;

namespace AstroPhotoGallery.Web.Utils.MapperConfiguration
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<CategoryMap>();
            });

            //Mapper.Configuration.AssertConfigurationIsValid();
        }
    }
}