using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Attributes;

namespace SteuerSoft.Network.Protocol.Payloads.Generic
{
    [WapPayload("msgs.Generic.StringList")]
    public class StringList
    {
        public List<string> List { get; set; } = new List<string>();
    }
}
