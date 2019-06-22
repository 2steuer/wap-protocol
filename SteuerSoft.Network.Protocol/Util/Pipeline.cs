using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteuerSoft.Network.Protocol.Util
{
    class Pipeline<T>
    {
        private readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _accessMutex = new SemaphoreSlim(1);

        private readonly LinkedList<T> _list = new LinkedList<T>();

        public async Task Add(T item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _accessMutex.WaitAsync(cancellationToken);

            _list.AddLast(item);
            _queueSemaphore.Release();

            _accessMutex.Release();
        }

        public async Task<T> Get(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _queueSemaphore.WaitAsync(cancellationToken);
            await _accessMutex.WaitAsync(cancellationToken);

            var item = _list.First.Value;
            _list.RemoveFirst();

            _accessMutex.Release();

            return item;
        }
    }
}
