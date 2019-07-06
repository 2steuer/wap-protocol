using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WapPayloadAttribute : Attribute
    {
        private static readonly Regex NameRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9\.\-_]*");

        public string Name { get; }

        public WapPayloadAttribute(string name)
        {
            if (!NameRegex.IsMatch(name))
            {
                throw new ArgumentException($"Invalid payload name '{name}'.");
            }

            Name = name;
        }
    }
}
