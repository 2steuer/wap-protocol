using System;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Communication.Base;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Interfaces;

namespace SteuerSoft.Network.Protocol.Client.Provider
{
    class WapConnection : WapProtocol
    {
        private Func<ReceivedWapMessage, Task<IWapMessage>> _methodCallHandler;
        private Func<ReceivedWapMessage, Task> _eventHandler;

        public WapConnection(Func<ReceivedWapMessage, Task<IWapMessage>> methodCallHandler,
            Func<ReceivedWapMessage, Task> eventHandler, ProviderStoppedDelegate stoppedHandler)
        {
            _methodCallHandler = methodCallHandler;
            _eventHandler = eventHandler;
            OnStopped += stoppedHandler;
        }

        protected override Task<IWapMessage> HandleMethodCall(ReceivedWapMessage msg)
        {
            return _methodCallHandler?.Invoke(msg);
        }

        protected override Task HandleEventMessage(ReceivedWapMessage msg)
        {
            return _eventHandler?.Invoke(msg);
        }
    }
}
