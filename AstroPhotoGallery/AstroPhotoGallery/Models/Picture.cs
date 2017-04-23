using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstroPhotoGallery.Models
{
    public class Picture
    {
        private ICollection<Tag> tags;

        public Picture()
        {
            this.tags = new HashSet<Tag>();
        }

        public Picture(string uploaderId, string title, string description, int categoryId)
        {
            this.PicUploaderId = uploaderId;
            this.PicTitle = title;
            this.PicDescription = description;
            this.CategoryId = categoryId;
            this.UploadDate = DateTime.Today;
            this.tags = new HashSet<Tag>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("Title")]
        public string PicTitle { get; set; }

        [Required]
        [StringLength(1300)]
        [DisplayName("Description")]
        public string PicDescription { get; set; }

        [ForeignKey("PicUploader")]
        public string PicUploaderId { get; set; }

        public virtual ApplicationUser PicUploader { get; set; }

        public DateTime UploadDate { get; set; }

        public string ImagePath { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public string CategoryName { get; set; }

        public virtual ICollection<Tag> Tags
        {
            get { return this.tags; }
            set { this.tags = value; }
        }

        public bool IsUploader(string name)
        {
            return this.PicUploader.UserName.Equals(name);
        }

        // Boolean variables required for the "NextPicture" and "PreviousPicture" implementation in Picture/Details:
        public bool IsLastOfCategory { get; set; }

        public bool IsFirstOfCategory { get; set; }

        public decimal RatingSum { get; set; }

        public int RatingCount { get; set; }

        public decimal Rating { get; set; }

        // String containing the user IDs that already rated the picture
        public string UserIdsRatedPic { get; set; } = string.Empty;
    }
}