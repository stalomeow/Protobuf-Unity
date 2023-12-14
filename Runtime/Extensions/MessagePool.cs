using System.Collections.Generic;
using System.Threading;
using static System.Threading.Interlocked;

namespace Google.Protobuf
{
    /// <summary>
    /// A thread-safe object pool for Protobuf Messages.
    /// </summary>
    public static class MessagePool
    {
        public static int MaxSize = 1000;
    }

    /// <summary>
    /// A thread-safe object pool for Protobuf Messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class MessagePool<T> where T : class, new()
    {
        private static T _fastItem;
        private static Stack<T> _restItems;
        private static SpinLock _spinLock;

        public static T Get()
        {
            // Fast Path
            T item = Exchange(ref _fastItem, null);

            if (item == null)
            {
                bool lockTaken = false;

                try
                {
                    _spinLock.Enter(ref lockTaken);

                    if (_restItems == null || !_restItems.TryPop(out item))
                    {
                        item = new T();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        _spinLock.Exit(useMemoryBarrier: false);
                    }
                }
            }

            return item;
        }

        public static void Release(T item)
        {
            if (item == null)
            {
                return;
            }

            // Fast Path
            if (CompareExchange(ref _fastItem, item, null) == null)
            {
                return;
            }

            bool lockTaken = false;

            try
            {
                _spinLock.Enter(ref lockTaken);

                _restItems ??= new Stack<T>();
                if (_restItems.Count < MessagePool.MaxSize)
                {
                    _restItems.Push(item);
                }
            }
            finally
            {
                if (lockTaken)
                {
                    _spinLock.Exit(useMemoryBarrier: false);
                }
            }
        }
    }
}
