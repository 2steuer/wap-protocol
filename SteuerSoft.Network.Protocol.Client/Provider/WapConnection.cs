using System;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Communication.Base;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.Interfaces;

namespace SteuerSoft.Network.Protocol.Client.Provider
{
    class WapConnection : WapProtocol
    {
        private Func<ReceivedWapMessage, Task<WapMessage>> _methodCallHandler;
        private Func<ReceivedWapMessage, Task> _eventHandler;

        public WapConnection(Func<ReceivedWapMessage, Task<WapMessage>> methodCallHandler,
            Func<ReceivedWapMessage, Task> eventHandler, ProviderStoppedDelegate stoppedHandler)
        {
            _methodCallHandler = methodCallHandler;
            _eventHandler = eventHandler;
            OnStopped += stoppedHandler;
        }

        protected override Task<WapMessage> HandleMethodCall(ReceivedWapMessage msg)
        {
            return _methodCallHandler?.Invoke(msg);
        }

        protected override Task HandleEventMessage(ReceivedWapMessage msg)
        {
            return _eventHandler?.Invoke(msg);
        }
    }
}
