using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Placeless
{
    public interface ICollector : IProgressReporter
    {
        Task Discover();
    }
}
