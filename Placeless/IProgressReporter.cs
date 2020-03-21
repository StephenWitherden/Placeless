using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Placeless
{
    public interface IProgressReporter
    {
        Task DoWork();

        IEnumerable<ProgressReport> GetReports();
    }
}
