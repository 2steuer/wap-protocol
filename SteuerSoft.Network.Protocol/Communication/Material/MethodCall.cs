using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Interfaces;

namespace SteuerSoft.Network.Protocol.Communication.Material
{
    class MethodCall
    {
        public IWapMessage Messsage { get; set; }
        public TaskCompletionSource<ReceivedWapMessage> CompletionSource { get; } = new TaskCompletionSource<ReceivedWapMessage>();
    }
}
