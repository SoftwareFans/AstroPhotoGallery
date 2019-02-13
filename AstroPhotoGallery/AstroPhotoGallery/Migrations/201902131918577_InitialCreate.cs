namespace AstroPhotoGallery.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.Pictures",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PicTitle = c.String(nullable: false, maxLength: 50),
                        PicDescription = c.String(nullable: false, maxLength: 1300),
                        PicUploaderId = c.String(maxLength: 128),
                        UploadDate = c.DateTime(nullable: false),
                        ImagePath = c.String(),
                        CategoryId = c.Int(nullable: false),
                        CategoryName = c.String(),
                        IsLastOfCategory = c.Boolean(nullable: false),
                        IsFirstOfCategory = c.Boolean(nullable: false),
                        RatingSum = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RatingCount = c.Int(nullable: false),
                        Rating = c.Decimal(nullable: false, precision: 18, scale: 2),
                        UserIdsRatedPic = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.PicUploaderId)
                .Index(t => t.PicUploaderId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Gender = c.String(),
                        Country = c.String(),
                        City = c.String(),
                        Birthday = c.String(),
                        ImagePath = c.String(),
                        IsEmailPublic = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.TagPictures",
                c => new
                    {
                        Tag_Id = c.Int(nullable: false),
                        Picture_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Tag_Id, t.Picture_Id })
                .ForeignKey("dbo.Tags", t => t.Tag_Id, cascadeDelete: true)
                .ForeignKey("dbo.Pictures", t => t.Picture_Id, cascadeDelete: true)
                .Index(t => t.Tag_Id)
                .Index(t => t.Picture_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.TagPictures", "Picture_Id", "dbo.Pictures");
            DropForeignKey("dbo.TagPictures", "Tag_Id", "dbo.Tags");
            DropForeignKey("dbo.Pictures", "PicUploaderId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Pictures", "CategoryId", "dbo.Categories");
            DropIndex("dbo.TagPictures", new[] { "Picture_Id" });
            DropIndex("dbo.TagPictures", new[] { "Tag_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Tags", new[] { "Name" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Pictures", new[] { "CategoryId" });
            DropIndex("dbo.Pictures", new[] { "PicUploaderId" });
            DropIndex("dbo.Categories", new[] { "Name" });
            DropTable("dbo.TagPictures");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Tags");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Pictures");
            DropTable("dbo.Categories");
        }
    }
}
