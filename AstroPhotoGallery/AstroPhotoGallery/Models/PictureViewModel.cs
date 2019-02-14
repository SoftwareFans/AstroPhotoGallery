using AstroPhotoGallery.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AstroPhotoGallery.Web.Models
{
    public class PictureViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Title")]
        public string PicTitle { get; set; }

        [Required]
        [StringLength(1300)]
        [DisplayName("Description")]
        public string PicDescription { get; set; }

        public string PicUploaderId { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        public ICollection<AstroPhotoGallery.Models.Category> Categories { get; set; }

        public string ImagePath { get; set; }

        [Required]
        [DisplayName("Tags separated by a space")]
        public string Tags { get; set; }
    }
}