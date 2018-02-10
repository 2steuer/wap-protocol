using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Client;
using SteuerSoft.Network.Protocol.Client.Material;
using SteuerSoft.Network.Protocol.Payloads.Generic;
using String = SteuerSoft.Network.Protocol.Payloads.Generic.String;

namespace MsgTests
{
    class TestClient : WapClient
    {
        private MethodCaller<Integer, Integer> _caller = null;
        private EventSubscriber<String> _sub = null;

        public TestClient(string controllerAddress, int port, string endPoint, string name) 
            : base(controllerAddress, port, endPoint, name)
        {
        }

        protected override async Task OnStart()
        {
            _caller = await CreateMethodCaller<Integer, Integer>(":testmethod");
            _sub = await SubscribeEvent<String>(":stringsforfun");
            _sub.OnReceived += _sub_OnReceived;
        }

        private void _sub_OnReceived(EventSubscriber<String> subscriber, String data)
        {
            Console.WriteLine(data.Value);
        }

        public async Task<int> CallMýMethod(int i)
        {
            Integer ip = new Integer() {Value = i};
            return (await _caller.Call(ip)).Value;
        }
    }
}
