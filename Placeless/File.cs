using System;
using System.Collections.Generic;
using System.Text;

namespace Placeless
{
    public class File
    {
        public int Id { get; set; }
        public IEnumerable<string> Metadata { get; set; }
    }
}
