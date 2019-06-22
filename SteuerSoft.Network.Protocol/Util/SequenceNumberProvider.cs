using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Util
{
    class SequenceNumberProvider
    {
        private static ulong _seq = 0;

        private SequenceNumberProvider()
        {

        }

        public static ulong Get()
        {
            return _seq++;
        }
    }
}
