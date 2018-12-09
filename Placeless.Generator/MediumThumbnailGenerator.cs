using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Placeless.Generator.Windows
{
    public class MediumThumbnailGenerator : ThumbnailGenerator
    {
        public override int MaxDimension => 640;

        public override string VersionTypeName => "Medium Thumbnail";

    }
}
