using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;


namespace AstroPhotoGallery.Models
{

    public class GalleryDbContext : IdentityDbContext<ApplicationUser>
    {
        public GalleryDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public virtual IDbSet<Picture> Pictures { get; set; }

        public virtual IDbSet<Category> Categories { get; set; }

        public virtual IDbSet<Tag> Tags { get; set; }

        public static GalleryDbContext Create()
        {
            return new GalleryDbContext();
        }
    }
}