using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Server.Material
{
    class ClientMethodInfo
    {
        public string ParamPayloadType { get; set; }
        public string ResultPayloadType { get; set; }
        public ClientConnection Client { get; set; }
    }
}
