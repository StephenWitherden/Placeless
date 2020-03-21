using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Placeless.Generator.Windows
{
    public class LargeThumbnailGenerator : ThumbnailGenerator
    {
        public override int MaxDimension => 320;

        public override string VersionTypeName => "Large Thumbnail";

    }
}
