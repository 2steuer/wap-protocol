using System;

namespace SteuerSoft.Network.Protocol.Client.Exceptions
{
    public class MethodCallFailedException : Exception
    {
        public MethodCallFailedException(string message)
            :base(message)
        {
            
        }
    }
}
