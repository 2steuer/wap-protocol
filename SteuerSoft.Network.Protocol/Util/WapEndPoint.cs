using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Util
{
    public class WapEndPoint
    {
        private readonly List<string> _parts = new List<string>();

        public static WapEndPoint Parse(string endPoint)
        {
            if (!endPoint.StartsWith(":"))
            {
                throw new ArgumentException("Absolute EndPoint name required");
            }

            WapEndPoint ep = new WapEndPoint();
            ep._parts.AddRange(endPoint.Substring(1).Split('.'));

            return ep;
        }

        public static WapEndPoint Parse(WapEndPoint baseEp, string endPoint)
        {
            if (endPoint.StartsWith(":"))
            {
                return Parse(endPoint);
            }
            else
            {
                WapEndPoint newEp = new WapEndPoint();
                newEp._parts.AddRange(baseEp._parts);
                if (endPoint.StartsWith("."))
                {
                    newEp._parts.Reverse();
                    while (endPoint.StartsWith("."))
                    {
                        newEp._parts.RemoveAt(0);
                        endPoint = endPoint.Substring(1);
                    }
                    newEp._parts.Reverse();
                }

                newEp._parts.AddRange(endPoint.Split('.'));
                return newEp;

            }
        }

        public override string ToString()
        {
            return $":{string.Join(".", _parts)}";
        }

        public override int GetHashCode()
        {
            int p = 17;

            foreach (string s in _parts)
            {
                unchecked
                {
                    p *= s.GetHashCode();
                }
            }

            return p;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            WapEndPoint ep = obj as WapEndPoint;
            
            if (ep == null || ep._parts.Count != _parts.Count)
            {
                return false;
            }

            for (int i = 0; i < _parts.Count; i++)
            {
                if (_parts[i] != ep._parts[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
