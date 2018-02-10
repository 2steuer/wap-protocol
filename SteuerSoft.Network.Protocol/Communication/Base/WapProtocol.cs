using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;

namespace SteuerSoft.Network.Protocol.Communication.Base
{
    public abstract class WapProtocol : WapMessageExchangeProvider
    {
        private Stream _stream = null;

        private SemaphoreSlim _methodSem = new SemaphoreSlim(1);
        private IWapMessage _currentMethodCall = null;
        private TaskCompletionSource<ReceivedWapMessage> _tcMethodCall = null;

        protected WapProtocol(Stream stream)
        {
            _stream = stream;
        }

        protected WapProtocol()
        {
            
        }

        public bool Start(Stream stream = null)
        {
            if (_stream == null && stream == null)
            {
                throw new ArgumentException("No working stream was provided in either the constructor or the start method");
            }
            else if (_stream == null && stream != null)
            {
                _stream = stream;
            }

            return StartHandler(_stream);
        }

        public bool Stop()
        {
            return StopHandler();
        }

        protected override async Task HandleMessage(ReceivedWapMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Event:
                    await HandleEventMessage(message);
                    break;

                case MessageType.MethodCall:
                    await SendMesssage(await HandleMethodCall(message));
                    break;

                case MessageType.MethodResponse:
                    if (_currentMethodCall != null)
                    {
                        if (_currentMethodCall.EndPoint == message.EndPoint)
                        {
                            _tcMethodCall.SetResult(message);
                        }
                    }
                    break;
            }
        }

        public async Task<ReceivedWapMessage> CallMethod(IWapMessage msg)
        {
            await _methodSem.WaitAsync();
            _currentMethodCall = msg;
            _tcMethodCall = new TaskCompletionSource<ReceivedWapMessage>();
            await SendMesssage(msg);
            var ret = await _tcMethodCall.Task;
            _currentMethodCall = null;
            _tcMethodCall = null;
            _methodSem.Release();

            return ret;
        }

        public Task<bool> SendEventMessage(IWapMessage msg)
        {
            return SendMesssage(msg);
        }

        protected abstract Task<IWapMessage> HandleMethodCall(ReceivedWapMessage msg);

        protected abstract Task HandleEventMessage(ReceivedWapMessage msg);


    }

    
}
