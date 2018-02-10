using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Attributes;

namespace SteuerSoft.Network.Protocol.Payloads.Control
{
    [WapPayload("msg.control.Auth")]
    public class Auth
    {
        public string EndPoint { get; set; }
        public string Name { get; set; }
    }
}
