using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDesk.Options;
using SteuerSoft.Network.Protocol.Util;
using SteuerSoft.Network.Protocol.Util.Logging;
using SteuerSoft.Network.Protocol.Util.Logging.ValueTypes;

namespace SteuerSoft.Network.Protocol.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            int port = 51234;
            LogLevel logLevel = LogLevel.Verbose;
            WapEndPoint ep = WapEndPoint.Parse(":control");
            OptionSet options = new OptionSet()
            {
                {"p|port=", "Port the server shall listen on",s => port = int.Parse(s) },
                {"l|log=", "The log level (starting from 0) for the console. The higher, the more information is printed.", s => logLevel = (LogLevel)int.Parse(s)},
                {"ep|endpoint=", "The base endpoint for the server.", s => ep = WapEndPoint.Parse(s) }
            };

            try
            {
                options.Parse(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured with the parameters. Please check below.");
                Console.WriteLine(ex.ToString());
                options.WriteOptionDescriptions(Console.Out);
                Environment.Exit(-1);
            }

            Log.ConsoleLevel = logLevel;
            var log = Log.Create("Main");

            Console.WriteLine("WapServer (c) 2018 Merlin Steuer");

            log.Info($"Program startup...");

            WapServer srv = new WapServer(port, ep.ToString());

            if (!srv.Start())
            {
                log.Fatal("Server startup failed.");
                Environment.Exit(-2);
            }

            ManualResetEvent ev = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                log.Info("Server shutting down...");
                srv.Stop();
                ev.Set();
            };

            
            ev.WaitOne();
        }
    }
}
