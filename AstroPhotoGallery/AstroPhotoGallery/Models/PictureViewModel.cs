using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AstroPhotoGallery.Models
{
    public class PictureViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Title")]
        public string PicTitle { get; set; }

        [Required]
        [DisplayName("Description")]
        public string PicDescription { get; set; }

        [ForeignKey("PicUploader")]
        public string PicUploaderId { get; set; }

        public string ImagePath { get; set; }
       
    }
}