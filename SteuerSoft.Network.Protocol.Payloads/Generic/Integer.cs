using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Attributes;

namespace SteuerSoft.Network.Protocol.Payloads.Generic
{
    [WapPayload("msgs.generic.Integer")]
    public class Integer
    {
        public int Value { get; set; }
    }
}
