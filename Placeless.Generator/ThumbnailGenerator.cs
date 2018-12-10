using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Placeless.Generator
{
    public abstract class ThumbnailGenerator : VersionGenerator
    {
        public abstract int MaxDimension { get; }

        public override string GenerateVersion(Stream input)
        {
            var image = Image.FromStream(input);
            Image thumb = image;
            if (image.Width >= image.Height && image.Width > MaxDimension)
            {
                thumb = image.GetThumbnailImage(MaxDimension, (image.Height * MaxDimension) / image.Width, () => false, IntPtr.Zero);
            }
            else if (image.Height > image.Width && image.Height > MaxDimension)
            {
                thumb = image.GetThumbnailImage((image.Width * MaxDimension) / image.Height, MaxDimension, () => false, IntPtr.Zero);
            }

            var ms = new MemoryStream();
            thumb.Save(ms, ImageFormat.Jpeg);
            ms.Seek(0, SeekOrigin.Begin);
            string result = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length, 0);
            return "data:image/jpeg;base64," + result;
        }

    }
}
