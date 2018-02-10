using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Util.Logging.Interfaces;
using SteuerSoft.Network.Protocol.Util.Logging.ValueTypes;

namespace SteuerSoft.Network.Protocol.Util.Logging
{
    public class Log : ILogger
    {
        public static LogLevel ConsoleLevel { get; set; } = LogLevel.Info;

        private string _topic;

        private Log(string topic)
        {
            _topic = topic;
        }

        public static ILogger Create(string topic)
        {
            return new Log(topic);
        }

        protected static void Add(LogLevel level, string topic, string message)
        {
            var dt = DateTime.Now;
            var str = $"[{dt:s}] ({level}/{topic}) {message}";

            if (level <= ConsoleLevel)
            {
                Console.WriteLine(str);
            }
        }

        protected static void Add(LogLevel level, string topic, Exception ex) => Add(level, topic, ex.ToString());

        public void Verbose(string message)
        {
            Add(LogLevel.Verbose, _topic, message);
        }

        public void Debug(string message)
        {
            Add(LogLevel.Debug, _topic, message);
        }

        public void Info(string message)
        {
            Add(LogLevel.Info, _topic, message);
        }

        public void Warning(string message)
        {
            Add(LogLevel.Warning, _topic, message);
        }

        public void Warning(Exception exception)
        {
            Add(LogLevel.Warning, _topic, exception);
        }

        public void Error(string message)
        {
            Add(LogLevel.Error, _topic, message);
        }

        public void Error(Exception exception)
        {
            Add(LogLevel.Error, _topic, exception);
        }

        public void Fatal(string message)
        {
            Add(LogLevel.Fatal, _topic, message);
        }

        public void Fatal(Exception exception)
        {
            Add(LogLevel.Fatal, _topic, exception);
        }
    }
}
