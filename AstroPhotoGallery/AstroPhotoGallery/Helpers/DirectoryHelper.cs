using System;
using System.IO;

namespace AstroPhotoGallery.Web.Helpers
{
    public static class DirectoryHelper
    {
        /// <summary>
        /// Rename directory
        /// </summary>
        /// <param name="catOldDir"></param>
        /// <param name="catNewDir"></param>
        public static void RenameDirectory(string catOldDir, string catNewDir)
        {
            if (!Directory.Exists(catOldDir))
            {
                throw new ArgumentException(nameof(catOldDir));
            }

            Directory.Move(catOldDir, catNewDir);
        }

        /// <summary>
        /// Create new directory, before this trim directory name
        /// </summary>
        /// <param name="directoryName">directory name</param>
        public static void CreateDirectory(string directoryName)
        {
            if (Directory.Exists(directoryName))
            {
                throw new ArgumentException(nameof(directoryName));
            }

            var trimEnd = directoryName.TrimEnd();
            var trimStart = trimEnd.TrimStart();
            Directory.CreateDirectory(trimStart);
        }

        /// <summary>
        /// Remove directory recursive if exist
        /// </summary>
        /// <param name="directoryName">directory name</param>
        public static void RemoveDirectory(string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                throw new ArgumentException(nameof(directoryName));
            }

            Directory.Delete(directoryName, true);
        }
    }
}