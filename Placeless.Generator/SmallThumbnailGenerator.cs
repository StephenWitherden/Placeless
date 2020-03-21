using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Placeless.Generator.Windows
{
    public class SmallThumbnailGenerator : ThumbnailGenerator
    {
        public override int MaxDimension => 80;

        public override string VersionTypeName => "Small Thumbnail";
    }
}
