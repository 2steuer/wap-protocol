using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Attributes;

namespace SteuerSoft.Network.Protocol.ExtensionMethods
{
    public static class TypeExtensions
    {
        public static string GetPayloadTypeName(this Type t)
        {
            var attr =
                t.GetCustomAttributes(typeof(WapPayloadAttribute), false).FirstOrDefault() as
                    WapPayloadAttribute;

            return attr?.Name ?? string.Empty;
        }
    }
}
