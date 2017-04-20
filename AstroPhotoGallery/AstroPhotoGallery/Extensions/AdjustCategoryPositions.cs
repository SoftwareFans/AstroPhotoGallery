using System.Linq;
using AstroPhotoGallery.Models;
using System.Data.Entity;

namespace AstroPhotoGallery.Extensions
{
    // Class for adjusting the positional boolean variables of a pic required for the "Next" and "Previous" buttons and methods

    public class AdjustCategoryPositions
    {
        public static void Upload(GalleryDbContext db, Picture picture)
        {
            // Get all IDs of the pics in the category of the pic being uploaded
            var allPicsIdsInCategory = db.Pictures
                .Where(p => p.CategoryId == picture.CategoryId)
                .Select(p => p.Id)
                .ToList();

            db.Pictures.Add(picture);
            db.SaveChanges();

            // In case there are already pics in that category before the upload
            if (allPicsIdsInCategory.Count != 0)
            {
                var first = allPicsIdsInCategory.Min(); // The first pic in the category
                var last = allPicsIdsInCategory.Max(); // The last pic in the category

                // If there is only 1 pic in the category before the upload
                if (first == last)
                {
                    var onlyPicInThatCategory = db.Pictures
                        .Where(p => p.Id == first)
                        .FirstOrDefault();

                    // The only picture is not last anymore because the newly uploaded one will have higher ID
                    onlyPicInThatCategory.IsLastOfCategory = false;

                    db.Entry(onlyPicInThatCategory).State = EntityState.Modified;

                    // The uploaded picture will have higher ID than the only picture before the upload so it will be last of category
                    picture.IsFirstOfCategory = false;
                    picture.IsLastOfCategory = true;
                }
                // If there are 2 or more pics in the category before the upload
                else
                {
                    // If the uploaded pic's ID is lower than than the first one before the upload
                    if (picture.Id < first)
                    {
                        picture.IsFirstOfCategory = true;

                        // Modifying the first pic in a category before the upload not to be first anymore
                        var previousMinPic = db.Pictures
                            .Where(p => p.Id == first)
                            .FirstOrDefault();

                        previousMinPic.IsFirstOfCategory = false;

                        db.Entry(previousMinPic).State = EntityState.Modified;

                    }
                    // If the uploaded pic's ID is higher than than the last one before the upload
                    else if (picture.Id > last)
                    {
                        picture.IsLastOfCategory = true;

                        // Modifying the last pic in a category before the upload not to be last anymore
                        var previousMaxPic = db.Pictures
                            .Where(p => p.Id == last)
                            .FirstOrDefault();

                        previousMaxPic.IsLastOfCategory = false;

                        db.Entry(previousMaxPic).State = EntityState.Modified;
                    }
                }
            }
            // In case there are no pics in the category before the currently uploaded one
            else
            {
                picture.IsFirstOfCategory = true;
                picture.IsLastOfCategory = true;
            }

            db.Entry(picture).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void Edit(GalleryDbContext db, Picture picture)
        {
            // Get all IDs of the pics in the category of the pic being edited
            var allPicsIdsInCategory = db.Pictures
                .Where(p => p.CategoryId == picture.CategoryId)
                .Select(p => p.Id)
                .ToList();

            // Remove the currently edited picture from the collection
            allPicsIdsInCategory.Remove(picture.Id);

            // If there are any pics other than the currently edited
            if (allPicsIdsInCategory.Count > 0)
            {
                var first = allPicsIdsInCategory.Min();   // The first pic in the category
                var last = allPicsIdsInCategory.Max();   // The last pic in the category

                // If there is only 1 pic in the category before the edit
                if (first == last)
                {
                    var onlyPicInThatCategory = db.Pictures
                        .Where(p => p.Id == first)
                        .FirstOrDefault();

                    // If the only picture's ID is lower than the edited one's then the edited one is now last in the category
                    if (onlyPicInThatCategory.Id < picture.Id)
                    {
                        onlyPicInThatCategory.IsFirstOfCategory = true;
                        onlyPicInThatCategory.IsLastOfCategory = false;

                        picture.IsFirstOfCategory = false;
                        picture.IsLastOfCategory = true;
                    }
                    // If the currently edited picture's ID is lower than the only picture then the edited one is now first in the category
                    else
                    {
                        onlyPicInThatCategory.IsFirstOfCategory = false;
                        onlyPicInThatCategory.IsLastOfCategory = true;

                        picture.IsFirstOfCategory = true;
                        picture.IsLastOfCategory = false;
                    }
                    db.Entry(onlyPicInThatCategory).State = EntityState.Modified;
                    db.Entry(picture).State = EntityState.Modified;
                }
                // If there are 2 or more pictures in the category before the edit
                else
                {
                    // If the edited pic's ID is lower than the first pic in the category then the edited pic becomes first
                    if (picture.Id < first)
                    {
                        picture.IsFirstOfCategory = true;
                        picture.IsLastOfCategory = false;

                        // Modifying the first pic in a category before the edit not to be first anymore
                        var previousMinPic = db.Pictures
                            .Where(p => p.Id == first)
                            .FirstOrDefault();

                        previousMinPic.IsFirstOfCategory = false;
                        previousMinPic.IsLastOfCategory = false;

                        db.Entry(previousMinPic).State = EntityState.Modified;

                    }
                    // If the edited pic's ID is higher than the last pic in the category then the edited pic becomes last
                    else if (picture.Id > last)
                    {
                        picture.IsLastOfCategory = true;
                        picture.IsFirstOfCategory = false;

                        // Modifying the last pic in a category before the edit not to be last anymore
                        var previousMaxPic = db.Pictures
                            .Where(p => p.Id == last)
                            .FirstOrDefault();

                        previousMaxPic.IsFirstOfCategory = false;
                        previousMaxPic.IsLastOfCategory = false;

                        db.Entry(previousMaxPic).State = EntityState.Modified;
                    }
                    // If the edited picture is between the first and the last then it is not first or last
                    else
                    {
                        picture.IsLastOfCategory = false;
                        picture.IsFirstOfCategory = false;
                    }
                }
            }
            // In case there are no pics in the category except the edited one then the edited one is first and last
            else
            {
                picture.IsFirstOfCategory = true;
                picture.IsLastOfCategory = true;
            }

            db.Entry(picture).State = EntityState.Modified;
            db.SaveChanges();
        }

        public static void Delete(GalleryDbContext db, Picture picture)
        {
            // Get the ID of the category of the pic that is being deleted
            var previousCategoryIdOfPic = picture.CategoryId;

            // Get the IDs of the pics from the category of the pic that is being deleted
            var previousCategoryPicsIdToBeChanged = db.Pictures
                   .Where(p => p.CategoryId == previousCategoryIdOfPic)
                   .Select(p => p.Id)
                   .ToList();

            // Remove the currently deleted pic from the collection
            previousCategoryPicsIdToBeChanged.Remove(picture.Id);

            // If there are any other pics in the category other than the one being deleted
            if (previousCategoryPicsIdToBeChanged.Count > 0)
            {
                var first = previousCategoryPicsIdToBeChanged.Min();   // The first pic in the category
                var last = previousCategoryPicsIdToBeChanged.Max();   // The last pic in the category

                // If there is only 1 pic in the category before the delete except the one being deleted then it becomes now first and last
                if (first == last)
                {
                    var onlyPicture = db.Pictures
                        .Where(p => p.Id == first)
                        .FirstOrDefault();

                    onlyPicture.IsLastOfCategory = true;
                    onlyPicture.IsFirstOfCategory = true;

                    db.Entry(onlyPicture).State = EntityState.Modified;
                }
                // If there are 2 or more pics in the category before the delete except the one being deleted
                else
                {
                    // The first pic is marked to be first now
                    var firstPic = db.Pictures.Where(p => p.Id == first).FirstOrDefault();
                    firstPic.IsFirstOfCategory = true;
                    firstPic.IsLastOfCategory = false;
                    db.Entry(firstPic).State = EntityState.Modified;

                    // The last pic is marked to be last now
                    var lastPic = db.Pictures.Where(p => p.Id == last).FirstOrDefault();
                    lastPic.IsFirstOfCategory = false;
                    lastPic.IsLastOfCategory = true;
                    db.Entry(lastPic).State = EntityState.Modified;
                }

                db.SaveChanges();
            }
        }
    }
}