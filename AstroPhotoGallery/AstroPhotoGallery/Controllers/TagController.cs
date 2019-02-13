using AstroPhotoGallery.Web.Extensions;
using AstroPhotoGallery.Models;
using PagedList;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using AstroPhotoGallery.Data;

namespace AstroPhotoGallery.Web.Controllers
{
    public class TagController : Controller
    {
        //
        // GET: Tag/ListPicsWithTag/id
        public ActionResult ListPicsWithTag(int? id, int? page)
        {
            if (id == null)
            {
                this.AddNotification("No tag ID provided.", NotificationType.ERROR);
                return RedirectToAction("Index", "Home");
            }

            using (var db = new GalleryDbContext())
            {
                var requestedTag = db.Tags
                    .Include(t => t.Pictures.Select(p => p.Tags))
                    .Include(t => t.Pictures.Select(p => p.PicUploader))
                    .FirstOrDefault(t => t.Id == id);

                if (requestedTag == null)
                {
                    this.AddNotification("Such a tag does not exist.", NotificationType.ERROR);
                    return RedirectToAction("Index", "Home");
                }

                //Get pictures from db
                var pictures = db.Tags
                    .Include(t => t.Pictures.Select(p => p.Tags))
                    .Include(t => t.Pictures.Select(p => p.PicUploader))
                    .FirstOrDefault(t => t.Id == id)
                    .Pictures
                    .OrderByDescending(p => p.Id)
                    .ToList();

                //Return the view
                int pageSize = 8;
                int pageNumber = (page ?? 1);
                return View(pictures.ToPagedList(pageNumber, pageSize));
            }
        }
    }
}