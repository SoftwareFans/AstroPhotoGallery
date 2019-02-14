using AstroPhotoGallery.Models;
using AstroPhotoGallery.Web.Models.Category;
using AutoMapper;

namespace AstroPhotoGallery.Web.Utility.MapperConfiguration.Maps
{
    public class CategoryMap : Profile
    {
        public CategoryMap()
        {
            #region Category

            CreateMap<AddEditCategoryViewModel, Category>()
                .ForMember(vm => vm.Name, map => map.MapFrom(m => m.Name))
                .ForMember(vm => vm.Id, map => map.MapFrom(m => m.Id))
                .ReverseMap();

            #endregion
        }
    }
}