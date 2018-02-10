using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Force.Crc32;
using SteuerSoft.Network.Protocol.Attributes;
using SteuerSoft.Network.Protocol.Client;
using SteuerSoft.Network.Protocol.Communication;
using SteuerSoft.Network.Protocol.Communication.Base;
using SteuerSoft.Network.Protocol.ExtensionMethods;
using SteuerSoft.Network.Protocol.Message;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Payloads.Generic;
using SteuerSoft.Network.Protocol.Server;
using SteuerSoft.Network.Protocol.Util;

namespace MsgTests
{
    class Program
    {
        static void Main(string[] args)
        {
            TestClient2 clt2 = new TestClient2("localhost", 51234, ":merlin.test2", "Merlin zweiter Client.");
            clt2.Connect().Wait();

            TestClient clt = new TestClient("localhost", 51234, ":merlin.test", "Merlins Test-Client!");
            clt.Connect().Wait();

            Console.WriteLine("Everything started.");
            Console.WriteLine("Calling with 16:");
            Console.WriteLine(clt.CallMýMethod(16).Result);

            Task.Delay(1000).Wait();
            Console.WriteLine("Getting event information:");

            InfoClient ic = new InfoClient("localhost", 51234, ":merlin.info", "Test Information client!");
            ic.Connect().Wait();

            Console.ReadLine();
        }



    }
}
