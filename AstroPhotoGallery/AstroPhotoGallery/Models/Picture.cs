using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace AstroPhotoGallery.Models
{
    public class Picture
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PicTitle { get; set; }

        [Required]
        public string PicDescription { get; set; }

        [ForeignKey("PicUploader")]
        public string PicUploaderId { get; set; }

        public string ImagePath { get; set; }     

        public virtual ApplicationUser PicUploader { get; set; }
    }
}