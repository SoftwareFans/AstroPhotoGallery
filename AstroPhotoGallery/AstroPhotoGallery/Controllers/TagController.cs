using AstroPhotoGallery.Extensions;
using AstroPhotoGallery.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace AstroPhotoGallery.Controllers
{
    public class TagController : Controller
    {
        //
        // GET: Tag/List/id
        public ActionResult List(int? id)
        {
            if (id == null)
            {
                this.AddNotification("No tag ID provided.", NotificationType.ERROR);
                return RedirectToAction("Index", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                //Get pictures from db
                var pictures = db.Tags
                    .Include(t => t.Pictures.Select(p => p.Tags))
                    .Include(t => t.Pictures.Select(p => p.PicUploader))
                    .FirstOrDefault(t => t.Id == id)
                    .Pictures
                    .ToList();

                //Return the view
                return View(pictures);
            }

        }
    }
}