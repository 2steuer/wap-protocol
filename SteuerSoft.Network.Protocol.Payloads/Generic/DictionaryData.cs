using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Attributes;

namespace SteuerSoft.Network.Protocol.Payloads.Generic
{
    [WapPayload("msg.generic.Dictionary")]
    public class DictionaryData
    {
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}
