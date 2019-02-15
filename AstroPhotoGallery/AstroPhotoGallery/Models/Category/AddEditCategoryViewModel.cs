using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AstroPhotoGallery.Common.Resources.Admin;
namespace AstroPhotoGallery.Web.Models.Category
{
    public class AddEditCategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(ResourceType = typeof(CategoryResources), Name = nameof(CategoryResources.lblName))]
        public string Name { get; set; }

        public string RequestType { get; set; }
    }
}   