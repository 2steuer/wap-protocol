using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WapPayloadAttribute : Attribute
    {
        public string Name { get; }

        public WapPayloadAttribute(string name)
        {
            Name = name;
        }
    }
}
