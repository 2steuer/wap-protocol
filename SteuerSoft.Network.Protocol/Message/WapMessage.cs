using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.ValueTypes;

namespace SteuerSoft.Network.Protocol.Message
{
    public class WapMessage<T> : WapMessage
    {
        static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            Formatting = Formatting.None
        };

        public T Payload { get; protected set; }

        public WapMessage(MessageType type, string endPoint, T payload) : base(type, endPoint)
        {
            Payload = payload;
        }

        public WapMessage(MessageType type, string endPoint, T payload, ulong sequenceNumber) : base(type, endPoint, sequenceNumber)
        {
            Payload = payload;
        }

        protected override byte[] GetPayload()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(Payload, _settings));
        }

        public static WapMessage<T> FromReceivedMessage(ReceivedWapMessage msg)
        {
            return new WapMessage<T>(msg.Type, msg.EndPoint, JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(msg.Payload), _settings));
        }
    }
}
