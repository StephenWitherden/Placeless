using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Placeless
{
    public interface ISource
    {
        IEnumerable<string> GetRoots();
        IEnumerable<DiscoveredFile> Discover(string path, HashSet<string> existingSources);
        Task<string> GetMetadata(string path);
        Stream GetContents(string path);
        string GetName();
    }
}
