using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AstroPhotoGallery.Models
{
    public class Category
    {
        private ICollection<Picture> pictures;

        public Category()
        {
            this.pictures = new HashSet<Picture>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Picture> Pictures { get; set; }
    }
}