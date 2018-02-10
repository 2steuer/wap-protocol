using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Attributes;

namespace SteuerSoft.Network.Protocol.Payloads.Control
{
    [WapPayload("msgs.Control.MethodInfoList")]
    public class MethodInfoList
    {
        public List<MethodInfo> List { get; set; } = new List<MethodInfo>();
    }
}
