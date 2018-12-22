using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Placeless
{
    public interface ISource
    {
        Task Discover();
        Task Retrieve();
        Task RefreshMetadata();
        string GetName();
    }
}
