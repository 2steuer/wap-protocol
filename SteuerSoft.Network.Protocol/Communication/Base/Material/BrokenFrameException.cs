using System;
using System.Collections.Generic;

namespace SteuerSoft.Network.Protocol.Communication.Base.Material
{
    class BrokenFrameException : Exception
    {
        public IEnumerable<byte> BrokenFrame { get; }

        public BrokenFrameException(string message, List<byte> data, Exception innerException = null)
            :base(message, innerException)
        {
            BrokenFrame = data;
        }
    }
}
