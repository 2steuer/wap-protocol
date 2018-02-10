using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Communication.Base;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Server.Material
{
    public delegate Task<IWapMessage> MethodCallHandlerDelegate(ClientConnection sender, ReceivedWapMessage msg);

    public delegate Task EventMessageHandlerDelegate(ClientConnection sender, ReceivedWapMessage msg);

    public class ClientConnection : WapProtocol
    {
        public WapEndPoint EndPoint { get; set; } = null;

        public string Name { get; set; }

        public List<string> Methods { get; } = new List<string>();
        public List<string> SubscribedEvents { get; } = new List<string>();
        public List<string> PublishingEvents { get; } = new List<string>();

        public bool IsAuthenticated
        {
            get { return EndPoint != null && !string.IsNullOrEmpty(EndPoint.ToString()); }
        }

        private MethodCallHandlerDelegate _methodHandler = null;
        private EventMessageHandlerDelegate _eventHandler = null;

        public ClientConnection(MethodCallHandlerDelegate methodHandler, EventMessageHandlerDelegate eventHandler,
            ProviderStoppedDelegate stoppedHandler)
        {
            OnStopped += stoppedHandler;
            _methodHandler = methodHandler;
            _eventHandler = eventHandler;
        }

        protected override Task<IWapMessage> HandleMethodCall(ReceivedWapMessage msg)
        {
            return _methodHandler?.Invoke(this, msg);
        }

        protected override Task HandleEventMessage(ReceivedWapMessage msg)
        {
            return _eventHandler?.Invoke(this, msg);
        }
    }
}
