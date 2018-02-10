using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.ExtensionMethods;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Client.Material
{
    public delegate Task PublishEvent<T>(EventPublisher<T> sender, T data) where T:new();

    public class EventPublisher<T> where T:new()
    {
        private PublishEvent<T> _publisher;

        public WapEndPoint Endpoint { get; }

        public string EventPayloadType => typeof(T).GetPayloadTypeName();

        internal EventPublisher(WapEndPoint ep, PublishEvent<T> publishDelegate)
        {
            Endpoint = ep;
            _publisher = publishDelegate;
        }

        public async Task Publish(T data)
        {
            await _publisher.Invoke(this, data);
        }

        public T CreateData()
        {
            return new T();
        }
    }
}
