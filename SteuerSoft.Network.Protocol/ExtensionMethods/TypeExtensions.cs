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

            if (attr == null)
            {
                return string.Empty;
            }

            if (t.IsGenericType)
            {
                var args = t.GetGenericArguments();

                var argStr = string.Join(",", args.Select(arg => arg.GetPayloadTypeName()));

                return $"{attr.Name}<{argStr}>";
            }
            else
            {
                return attr.Name;
            }
        }
    }
}
