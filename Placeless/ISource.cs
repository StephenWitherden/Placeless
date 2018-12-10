using System;
using System.Threading.Tasks;

namespace Placeless
{
    public interface ISource
    {
        Task Discover();
        Task Retrieve();
        string GetName();
    }
}
