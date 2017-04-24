using System.Collections.Generic;
using System.IO;

namespace AstroPhotoGallery.Extensions
{
    // Class for image validation based on the headers of the files themselves. Even if a user change the extension of a file to .jpg for example, the file won't be accepted - its file header won't be corresponding to the headers of image types JPG, JPEG, PNG, BMP, ICO (others can be easily added in method IsStreamValid).

    public class ImageValidator
    {
        // Primary method for checking if the image is in valid format:
        public static bool IsImageValid(string filePath)
        {
            return File.Exists(filePath) &&
                   IsStreamValid(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        // Method for IsImageValid - checking the data passed to the stream:
        public static bool IsStreamValid(FileStream inputStream)
        {
            if (inputStream.Length > 0) // If the file has any data at all 
            {
                using (inputStream)
                {
                    string fileExt = inputStream.Name.Substring(inputStream.Name.LastIndexOf('.') + 1).ToUpper();

                    if (!(fileExt.Equals("JPG") ||
                        fileExt.Equals("JPEG") ||
                        fileExt.Equals("PNG") ||
                        fileExt.Equals("BMP") ||
                        fileExt.Equals("ICO")
                        ))
                    {
                        return false;
                    }

                    Dictionary<string, List<byte[]>> imageHeaders = new Dictionary<string, List<byte[]>>();

                    // For every image type there is a list of byte arrays with its headers possible values.
                    // Other image types can be added in case of need - TIF, GIF etc.

                    switch (fileExt)
                    {
                        case "JPG":
                            imageHeaders.Add("JPG", new List<byte[]>());
                            imageHeaders["JPG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
                            imageHeaders["JPG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 });
                            imageHeaders["JPG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 });
                            imageHeaders["JPG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xFE });
                            break;

                        case "JPEG":
                            imageHeaders.Add("JPEG", new List<byte[]>());
                            imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
                            imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 });
                            imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 });
                            imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 });
                            imageHeaders["JPEG"].Add(new byte[] { 0xFF, 0xD8, 0xFF, 0xFE });
                            break;

                        case "PNG":
                            imageHeaders.Add("PNG", new List<byte[]>());
                            imageHeaders["PNG"].Add(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
                            break;

                        case "BMP":
                            imageHeaders.Add("BMP", new List<byte[]>());
                            imageHeaders["BMP"].Add(new byte[] { 0x42, 0x4D });
                            break;

                        case "ICO":
                            imageHeaders.Add("ICO", new List<byte[]>());
                            imageHeaders["ICO"].Add(new byte[] { 0x00, 0x00, 0x01, 0x00 });
                            break;
                    }

                    foreach (var subType in imageHeaders[fileExt])
                    {
                        byte[] standardHeader = subType;
                        byte[] checkedHeader = new byte[standardHeader.Length];
                        inputStream.Position = 0;
                        inputStream.Read(checkedHeader, 0, checkedHeader.Length);

                        if (AreTwoByteArrsEqual(standardHeader, checkedHeader))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            else
            {
                inputStream.Dispose();
                return false; // the file is empty
            }
        }

        // Method for IsImageValid - comparing two byte arrays:
        public static bool AreTwoByteArrsEqual(byte[] firstArr, byte[] secondArr)
        {
            if (firstArr.Length != secondArr.Length)
            {
                return false;
            }

            for (int i = 0; i < firstArr.Length; i++)
            {
                if (firstArr[i] != secondArr[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}