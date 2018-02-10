using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Startable
{
    public interface IStartable
    {
        Task Start(IEnumerable<string> args);
        void Stop();
    }
}
