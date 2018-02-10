using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Util
{
    public static class NumberConverter
    {
        private static bool _flip = false;

        public static bool TargetLittleEndian
        {
            get
            {
                return !_flip && BitConverter.IsLittleEndian;
            }
            set { _flip = value != BitConverter.IsLittleEndian; }
        }

        static NumberConverter()
        {
            TargetLittleEndian = true;
        }

        public static byte[] GetBytes(ushort val)
        {
            var ret = BitConverter.GetBytes(val);
            if (_flip)
            {
                Array.Reverse(ret);
            }

            return ret;
        }

        public static ushort ToUInt16(byte[] data, int startIndex)
        {
            if (!_flip)
            {
                return BitConverter.ToUInt16(data, startIndex);
            }
            else
            {
                return BitConverter.ToUInt16(data.Skip(startIndex).Take(2).Reverse().ToArray(), 0);
            }
        }
    }
}
