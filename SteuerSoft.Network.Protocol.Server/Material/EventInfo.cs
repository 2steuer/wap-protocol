using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Server.Material
{
    class EventInfo
    {
        public string PayloadType { get; set; }
        public List<ClientConnection> Subscribtions { get; } = new List<ClientConnection>();
        public List<ClientConnection> Publishers { get; } = new List<ClientConnection>();
    }
}
