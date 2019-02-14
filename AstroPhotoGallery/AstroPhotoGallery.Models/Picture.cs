using System;
using System.Collections.Generic;
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

        //TODO: remove this shit
        public Picture(string uploaderId, string title, string description, int categoryId)
        {
            this.PicUploaderId = uploaderId;
            this.PicTitle = title;
            this.PicDescription = description;
            this.CategoryId = categoryId;
            this.UploadDate = DateTime.UtcNow.Date;
            this.tags = new HashSet<Tag>();
        }

        public int Id { get; set; }

        public string PicTitle { get; set; }

        public string PicDescription { get; set; }

        [ForeignKey(nameof(PicUploader))]
        public string PicUploaderId { get; set; }

        public virtual ApplicationUser PicUploader { get; set; }

        public DateTime UploadDate { get; set; }

        public string ImagePath { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        //TODO: remove we have this in virtual porperty
        public string CategoryName { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        //Remove this... 
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