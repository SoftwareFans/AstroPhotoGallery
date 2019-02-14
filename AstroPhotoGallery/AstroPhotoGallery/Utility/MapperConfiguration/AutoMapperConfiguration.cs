using AstroPhotoGallery.Web.Utility.MapperConfiguration.Maps;
using AutoMapper;

namespace AstroPhotoGallery.Web.Utility.MapperConfiguration
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