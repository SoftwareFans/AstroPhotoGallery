using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AstroPhotoGallery.Models
{
   

    public class GalleryDbContext : IdentityDbContext<ApplicationUser>
    {
        public GalleryDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static GalleryDbContext Create()
        {
            return new GalleryDbContext();
        }
    }
}