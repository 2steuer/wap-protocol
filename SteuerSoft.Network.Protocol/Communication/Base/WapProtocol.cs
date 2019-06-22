using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Communication.Material;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Communication.Base
{
    public abstract class WapProtocol : WapMessageExchangeProvider
    {
        private Stream _stream = null;

        private AsyncDictionary<ulong, MethodCall> _methodCalls = new AsyncDictionary<ulong, MethodCall>();

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
                    var msg = await HandleMethodCall(message);
                    msg.SetSequenceNumber(message.SequenceNumber);
                    await SendMesssage(msg);
                    break;

                case MessageType.MethodResponse:
                    var call = await _methodCalls.Get(message.SequenceNumber);

                    if ((call != null) && (call.Messsage.EndPoint == message.EndPoint))
                    {
                        call.CompletionSource.SetResult(message);
                    }
                    break;
            }
        }

        public async Task<ReceivedWapMessage> CallMethod(IWapMessage msg, CancellationToken ct = default(CancellationToken))
        {
            MethodCall call = new MethodCall();
            call.Messsage = msg;
            var hdlr = ct.Register(call.CompletionSource.SetCanceled);
            await _methodCalls.Add(msg.SequenceNumber, call, ct);
            try
            {
                await SendMesssage(msg);
                var ret = await call.CompletionSource.Task;
                return ret;
            }
            finally
            {
                hdlr.Dispose();
                await _methodCalls.Remove(msg.SequenceNumber); // cancellation ignored as we usually get here when the task is cancelled already but we still want to remove the call
            }
        }

        public Task<bool> SendEventMessage(IWapMessage msg)
        {
            return SendMesssage(msg);
        }

        protected abstract Task<WapMessage> HandleMethodCall(ReceivedWapMessage msg);

        protected abstract Task HandleEventMessage(ReceivedWapMessage msg);


    }

    
}
