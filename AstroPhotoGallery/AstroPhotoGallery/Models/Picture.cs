using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [DisplayName("Title")]
        public string PicTitle { get; set; }

        [Required]
        [DisplayName("Description")]
        public string PicDescription { get; set; }

        [ForeignKey("PicUploader")]
        public string PicUploaderId { get; set; }

        public string ImagePath { get; set; }

        public virtual ApplicationUser PicUploader { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }

        public ICollection<Category> Categories { get; set; }

        public string CategoryName { get; set; }
       
        public string Votes { get; set; }

        public bool IsUploader(string name)
        {
            return this.PicUploader.UserName.Equals(name);
        }

        // Boolean variables required for the "NextPicture" and "PreviousPicture" implementation in Picture/Details:
        public bool IsLastOfCategory { get; set; } 

        public bool IsFirstOfCategory { get; set; }
    }
}