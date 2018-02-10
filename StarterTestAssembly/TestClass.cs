using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Startable;

namespace StarterTestAssembly
{
    [Export(typeof(IStartable))]
    public class TestClass : IStartable
    {
        public Task Start(IEnumerable<string> args)
        {
            Console.WriteLine("Starting the Plugin!");
            int counter = 0;
            foreach (var arg in args)
            {
                Console.WriteLine($"[{counter++}] {arg}");
            }

            return Task.FromResult(true);
        }

        public void Stop()
        {
            Console.WriteLine("Stopping the Plugin!");
        }
    }
}
