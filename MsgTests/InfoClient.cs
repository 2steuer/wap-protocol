using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Client;
using SteuerSoft.Network.Protocol.Payloads.Control;
using SteuerSoft.Network.Protocol.Payloads.Generic;
using SteuerSoft.Network.Protocol.Util;
using String = SteuerSoft.Network.Protocol.Payloads.Generic.String;

namespace MsgTests
{
    class InfoClient : WapClient
    {
        public InfoClient(string controllerAddress, int port, string endPoint, string name) : base(controllerAddress, port, endPoint, name)
        {
        }

        protected override async Task OnStart()
        {
            var i = await CallControlMethod<Empty, DictionaryData>("events.list", new Empty());

            Console.WriteLine("List of Events:");
            foreach (var kvp in i.Data)
            {
                Console.WriteLine($"--- {kvp.Key} ({kvp.Value})");
                Console.WriteLine("Publishers:");

                foreach (var str in (await CallControlMethod<String, StringList>("events.getpublishers", new String() {Value = kvp.Key})).List)
                {
                    Console.WriteLine($"-- {str}");
                }
                Console.WriteLine();
                Console.WriteLine("Subscribers:");

                foreach (var str in (await CallControlMethod<String, StringList>("events.getsubscribers", new String() { Value = kvp.Key })).List)
                {
                    Console.WriteLine($"-- {str}");
                }
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("List of methods:");

            foreach (
                var il in
                    (await CallControlMethod<Empty, MethodInfoList>("methods.list", new Empty())).List.OrderBy(mi => mi.EndPoint))
            {
                Console.WriteLine($"-- {il.EndPoint} ({il.ParamPayloadType}) -> {il.ResultPayloadType}");
            }
            
        }
    }
}
