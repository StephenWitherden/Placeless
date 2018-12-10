using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Placeless
{
    public abstract class VersionGenerator
    {
        public abstract string VersionTypeName { get; }
        public abstract string GenerateVersion(Stream file);
    }
}
