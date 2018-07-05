using System.ComponentModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AstroPhotoGallery.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("First name")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Last name")]
        public string LastName { get; set; }

        public string Gender { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        // property PhoneNumber is part of IdentityUser by default: 
        // IdentityUser<TKey, TLogin, TRole, TClaim> - public virtual string PhoneNumber { get; set; }

        public string Birthday { get; set; } = string.Empty; // default value when creating a user in order not to be null

        public string ImagePath { get; set; }

        public bool IsEmailPublic { get; set; } = false; // setting the default to be false due to privacy reasons

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}