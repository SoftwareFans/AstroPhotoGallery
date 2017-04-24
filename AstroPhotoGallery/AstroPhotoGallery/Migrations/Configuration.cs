using System.Collections.Generic;
using System.IO;
using System.Web;

namespace AstroPhotoGallery.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using AstroPhotoGallery.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    public sealed class Configuration : DbMigrationsConfiguration<AstroPhotoGallery.Models.GalleryDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(GalleryDbContext context)
        {
            if (!context.Roles.Any())
            {
                this.CreateRole(context, "Admin");
                this.CreateRole(context, "User");
            }

            if (!context.Users.Any())
            {
                this.CreateUser(context, "admin@astrogallery.net", "Astrogallery", "Administrator", "123");
                this.SetRoleToUser(context, "admin@astrogallery.net", "Admin");
            }

            if (!context.Categories.Any())
            {
                this.CreateCategory(context, "Other");
            }
        }

        private void SetRoleToUser(GalleryDbContext context, string email, string role)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var user = context.Users.Where(u => u.Email == email).First();

            var result = userManager.AddToRole(user.Id, role);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }

        private void CreateUser(GalleryDbContext context, string email, string firstName, string lastName, string password)
        {
            //Create user manager 
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            //Set user manager password validator
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 3,
                RequireDigit = false,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false
            };

            //Create user object
            var admin = new ApplicationUser
            {
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };

            //Create user 
            var result = userManager.Create(admin, password);

            //Validate result

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }

        private void CreateRole(GalleryDbContext context, string roleName)
        {
            var roleManager = new RoleManager<IdentityRole>(
               new RoleStore<IdentityRole>(context));

            var result = roleManager.Create(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors));
            }
        }

        private void CreateCategory(GalleryDbContext context, string name)
        {
            if (!context.Categories.Any())
            {
                var category = new Category
                {
                    Name = name,
                    Pictures = new HashSet<Picture>()
                };

                context.Categories.Add(category);
                context.SaveChanges();

                var categoryDir = HttpContext.Current.Server.MapPath($"~/Content/images/astroPics/{category.Name}");
                Directory.CreateDirectory(categoryDir);
            }
        }
    }
}
