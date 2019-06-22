using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Communication.Material;
using SteuerSoft.Network.Protocol.ExtensionMethods;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Server.Material;
using SteuerSoft.Network.Protocol.Server.Util.MethodProxy.Material;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Server.Util.MethodProxy
{
    public class ServerMethodProxy
    {
        private Dictionary<string, Func<ClientConnection, ReceivedWapMessage, Task<WapMessage>>> _methods = new Dictionary<string, Func<ClientConnection, ReceivedWapMessage, Task<WapMessage>>>();
        private Dictionary<string, MethodInfo> _infos = new Dictionary<string, MethodInfo>();

        internal Dictionary<string, MethodInfo> Methods => _infos;

        public bool HasMethod(string endpoint)
        {
            return HasMethod(WapEndPoint.Parse(endpoint));
        }

        public bool HasMethod(WapEndPoint ep)
        {
            var str = ep.ToString();

            return _methods.ContainsKey(str) && _infos.ContainsKey(str);
        }

        public bool AddMethod<TParam, TResult>(WapEndPoint ep, Func<ClientConnection, TParam, Task<MethodResult<TResult>>> func)
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

            var f = new Func<ClientConnection, ReceivedWapMessage, Task<WapMessage>>(async (client, message) =>
            {
                WapMessage<TParam> msg = WapMessage<TParam>.FromReceivedMessage(message);

                var result = await func(client, msg.Payload);

                WapMessage<MethodResult<TResult>> res = new WapMessage<MethodResult<TResult>>(MessageType.MethodResponse, message.EndPoint, result);
                return res;
            });

            _infos.Add(str, i);
            _methods.Add(str, f);

            return true;
        }


        public void RemoveMethod(string endpoint)
        {
            if (!_methods.ContainsKey(endpoint))
            {
                return;
            }

            _methods.Remove(endpoint);
            _infos.Remove(endpoint);
        }

        public async Task<WapMessage> CallMethod(ClientConnection client, WapEndPoint ep, ReceivedWapMessage msg)
        {
            if (!HasMethod(ep))
            {
                return null;
            }

            return await _methods[ep.ToString()](client, msg);
        }
        
    }
}
