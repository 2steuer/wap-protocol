using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Client.Interfaces;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Client.Material
{
    public delegate void EventReceivedDelegate<TEvent>(EventSubscriber<TEvent> subscriber, TEvent data);

    public class EventSubscriber<TEvent> : IEventSubscriber
    {
        public WapEndPoint Endpoint { get; }

        public event EventReceivedDelegate<TEvent> OnReceived;

        internal EventSubscriber(WapEndPoint endpoint)
        {
            Endpoint = endpoint;
        }

        internal void PassData(TEvent data)
        {
            OnReceived?.Invoke(this, data);
        }
    }
}
