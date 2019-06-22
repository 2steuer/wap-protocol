using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Util
{
    class AsyncDictionary<TKey, TValue>
    {
        private SemaphoreSlim _mutex = new SemaphoreSlim(1);

        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        public async Task Add(TKey key, TValue value, CancellationToken ct = default(CancellationToken))
        {
            await _mutex.WaitAsync(ct);
            _dict.Add(key, value);
            _mutex.Release();
        }

        public async Task<TValue> Get(TKey key, CancellationToken ct = default(CancellationToken))
        {
            await _mutex.WaitAsync(ct);
            TValue ret = default(TValue);

            if (_dict.ContainsKey(key))
            {
                ret = _dict[key];
            }

            _mutex.Release();
            return ret;
        }

        public async Task Remove(TKey key, CancellationToken ct = default(CancellationToken))
        {
            await _mutex.WaitAsync(ct);
            _dict.Remove(key);
            _mutex.Release();
        }
    }
}
