using System.ComponentModel.DataAnnotations;

namespace AstroPhotoGallery.Models.Category
{
    public class AddEditCategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string RequestType { get; set; }
    }
}   