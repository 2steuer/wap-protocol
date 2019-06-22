using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Message.ValueTypes;

namespace SteuerSoft.Network.Protocol.Message.Interfaces
{
    public interface IWapMessage
    {
        MessageType Type { get; }
        ulong SequenceNumber { get; }
        string EndPoint { get; }

        byte[] GetBytes();
    }
}
