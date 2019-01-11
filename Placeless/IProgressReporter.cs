using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless
{
    public interface IProgressReporter
    {
        IEnumerable<ProgressReport> GetReports();
    }
}
