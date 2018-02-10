using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Client.Material
{
    public class MethodCaller<TParam, TResult>
    {
        private Func<WapEndPoint, TParam, Task<TResult>> _caller;

        public WapEndPoint EndPoint { get; }

        internal MethodCaller(Func<WapEndPoint, TParam, Task<TResult>> caller, WapEndPoint endPoint)
        {
            _caller = caller;
            EndPoint = endPoint;
        }

        public Task<TResult> Call(TParam param)
        {
            return _caller(EndPoint, param);
        }
    }
}
