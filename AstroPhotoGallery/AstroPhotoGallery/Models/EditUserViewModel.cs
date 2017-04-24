using System.Collections.Generic;
using System.ComponentModel;

namespace AstroPhotoGallery.Models
{
    public class EditUserViewModel
    {
        public virtual ApplicationUser User { get; set; }

        public string Password { get; set; }

        [DisplayName("Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Passwords does not match.")]
        public string ConfirmPassword { get; set; }

        public IList<Role> Roles { get; set; }
    }
}