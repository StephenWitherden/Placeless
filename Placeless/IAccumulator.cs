using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless
{
    public interface IRootAccumulator
    {
        void RootDiscovered(string path);
        void RootDiscovered(string[] paths);
        void FileDiscovered(DiscoveredFile file);
        void FileDiscovered(DiscoveredFile[] files);
    }
}
