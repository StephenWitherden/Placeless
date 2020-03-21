using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Placeless
{
    public interface IBlobStore
    {
        Stream Get(int id);
        Task PutAsync(Stream contents, int id);
    }
}
