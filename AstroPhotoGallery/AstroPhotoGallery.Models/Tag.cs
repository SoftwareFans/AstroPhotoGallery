using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AstroPhotoGallery.Models
{
    public class Tag
    {
        private ICollection<Picture> pictures;

        public Tag()
        {
            this.pictures = new HashSet<Picture>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(20)]
        [Index(IsUnique = true)]
        public string Name { get; set; }

        public virtual ICollection<Picture> Pictures
        {
            get { return this.pictures; }
            set { this.pictures = value; }
        }
    }
}