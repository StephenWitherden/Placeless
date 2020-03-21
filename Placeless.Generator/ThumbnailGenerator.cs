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

        private readonly ImageCodecInfo _jpegCodec;
        private readonly EncoderParameters _encoderParameters;

        public ThumbnailGenerator()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    _jpegCodec = codec;
                }
            }

            var encoderParameter = new EncoderParameter(myEncoder, 100L);

            _encoderParameters = new EncoderParameters(1);
            _encoderParameters.Param[0] = encoderParameter;
        }

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
            thumb.Save(ms, _jpegCodec, _encoderParameters);
            ms.Seek(0, SeekOrigin.Begin);
            string result = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length, 0);
            return result;
        }

    }
}
