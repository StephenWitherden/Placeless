using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless
{
    public interface IUserInteraction
    {
        void OpenWebPage(string url);
        string InputPrompt(string prompt);
        void ReportStatus(string status);
        void ReportError(string error);
    }
}
