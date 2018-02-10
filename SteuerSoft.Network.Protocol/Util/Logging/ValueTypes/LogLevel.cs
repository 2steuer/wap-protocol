using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Util.Logging.ValueTypes
{
    public enum LogLevel : int
    {
        Fatal = 0,
        Error,
        Warning,
        Info,
        Debug,
        Verbose,
        ConnectionLevel
    }
}
