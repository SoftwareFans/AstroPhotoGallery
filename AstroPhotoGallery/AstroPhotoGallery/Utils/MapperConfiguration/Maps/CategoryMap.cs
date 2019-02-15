using AstroPhotoGallery.Models;
using AstroPhotoGallery.Web.Models.Category;
using AutoMapper;

namespace AstroPhotoGallery.Web.Utils.MapperConfiguration.Maps
{
    public class CategoryMap : Profile
    {
        public CategoryMap()
        {
            CreateMap<AddEditCategoryViewModel, Category>()
                .ForMember(vm => vm.Name, map => map.MapFrom(m => m.Name))
                .ForMember(vm => vm.Id, map => map.MapFrom(m => m.Id))
                .ReverseMap();

            CreateMap<DeleteCategoryViewModel, Category>()
              .ForMember(vm => vm.Name, map => map.MapFrom(m => m.Name))
              .ForMember(vm => vm.Id, map => map.MapFrom(m => m.Id))
              .ReverseMap();
        }
    }
}