using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AstroPhotoGallery.Models
{
    public class PictureViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Title")]
        public string PicTitle { get; set; }

        [Required]
        [DisplayName("Description")]
        public string PicDescription { get; set; }

        public string PicUploaderId { get; set; }

        public int CategoryId { get; set; }

        public ICollection<Category> Categories { get; set; }

        public string ImagePath { get; set; }

        public string Tags { get; set; }
    }
}