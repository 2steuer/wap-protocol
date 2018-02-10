using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Client.Util.MethodProxy.Material;
using SteuerSoft.Network.Protocol.Communication.Material;
using SteuerSoft.Network.Protocol.ExtensionMethods;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Client.Util.MethodProxy
{
    public class MethodProxy
    {
        private Dictionary<string, Func<ReceivedWapMessage, Task<IWapMessage>>> _methods = new Dictionary<string, Func<ReceivedWapMessage, Task<IWapMessage>>>();
        private Dictionary<string, MethodInfo> _infos = new Dictionary<string, MethodInfo>();

        public bool HasMethod(string endpoint)
        {
            return HasMethod(WapEndPoint.Parse(endpoint));
        }

        public bool HasMethod(WapEndPoint ep)
        {
            var str = ep.ToString();

            return _methods.ContainsKey(str) && _infos.ContainsKey(str);
        }

        public bool AddMethod<TParam, TResult>(WapEndPoint ep, Func<TParam, Task<TResult>> func)
        {
            if (HasMethod(ep))
            {
                return false;
            }

            var str = ep.ToString();

            var i = new MethodInfo()
            {
                ParameterPayloadType = typeof(TParam).GetPayloadTypeName(),
                ResultPayloadType = typeof(TResult).GetPayloadTypeName()
            };

            var f = new Func<ReceivedWapMessage, Task<IWapMessage>>(async message =>
            {
                WapMessage<TParam> msg = WapMessage<TParam>.FromReceivedMessage(message);

                var result = new MethodResult<TResult>()
                {
                    Success = true,
                    Result = await func(msg.Payload)
                };

                WapMessage<MethodResult<TResult>> res = new WapMessage<MethodResult<TResult>>(MessageType.MethodResponse, message.EndPoint, result);
                return res;
            });

            _infos.Add(str, i);
            _methods.Add(str, f);

            return true;
        }

        public async Task<IWapMessage> CallMethod(WapEndPoint ep, ReceivedWapMessage msg)
        {
            if (!HasMethod(ep))
            {
                return null;
            }

            return await _methods[ep.ToString()](msg);
        }
        
    }
}
