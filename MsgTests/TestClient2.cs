using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using SteuerSoft.Network.Protocol.Client;
using SteuerSoft.Network.Protocol.Client.Material;
using SteuerSoft.Network.Protocol.Payloads.Generic;
using SteuerSoft.Network.Protocol.Payloads;
using String = SteuerSoft.Network.Protocol.Payloads.Generic.String;

namespace MsgTests
{
    class TestClient2 : WapClient
    {
        private EventPublisher<String> _pub = null;

        private Timer _t = new Timer(1000);
        private int _counter = 0;

        public TestClient2(string controllerAddress, int port, string endPoint, string name) : base(controllerAddress, port, endPoint, name)
        {
        }

        protected override async Task OnStart()
        {
            await RegisterMethod<Integer, Integer>(":testmethod", ip =>
            {
                var newIp = new Integer() {Value = ip.Value * ip.Value};

                return Task.FromResult(newIp);
            });

            _pub = await CreatePublisher<String>(":stringsforfun");
            _t.Elapsed += _t_Elapsed;
            _t.AutoReset = true;
        }

        private void _t_Elapsed(object sender, ElapsedEventArgs e)
        {
            var p = _pub.CreateData();
            p.Value = $"This is transmission {_counter++}!";

            _pub.Publish(p).Wait();
        }
    }
}
