using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Startable;

namespace SteuerSoft.Network.Protocol.Starter
{
    class Program
    {
        [Import]
        private IStartable _node;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run(args);
        }

        private void Run(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No startup assembly given.");
                Environment.Exit(-1);
            }

            var assbly = args[0];
            var param = args.Skip(1);

            try
            {
                AssemblyCatalog cat = new AssemblyCatalog(Assembly.LoadFrom(assbly));
                CompositionContainer cont = new CompositionContainer(cat);
                cont.ComposeParts(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while loading assembly.");
                Console.WriteLine(ex);
                Environment.Exit(-1);
            }

            if (_node == null)
            {
                Console.WriteLine("Assembly loaded, but no IStartable found.");
                Environment.Exit(-1);
            }

            ManualResetEvent ev = new ManualResetEvent(false);

            try
            {
                _node.Start(param).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while starting ...");
                Console.WriteLine(ex);
            }

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                _node.Stop();
                ev.Set();
            };

            ev.WaitOne();
        }
    }
}
