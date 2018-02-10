using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Client.Material
{
    public class MethodProvider<TParam, TResult>
    {
        public WapEndPoint Endpoint { get; }
        public Func<TParam, Task<TResult>> Func { get; }

        internal MethodProvider(WapEndPoint ep, Func<TParam, Task<TResult>> func)
        {
            Endpoint = ep;
            Func = func;
        }
    }
}
