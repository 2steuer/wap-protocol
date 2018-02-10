using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Attributes;

namespace SteuerSoft.Network.Protocol.Payloads.Generic
{
    [WapPayload("msgs.generic.String")]
    public class String
    {
        public string Value { get; set; }
    }
}
