using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless
{
    public interface IPlacelessconfig
    {
        IEnumerable<string> GetValues(string path);
        string GetValue(string path);
        void SetValues(string path, IEnumerable<string> values);
        void SetValue(string path, string value);
    }
}
