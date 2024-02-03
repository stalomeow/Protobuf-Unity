using System.Collections.Generic;
using System.Threading;
using static System.Threading.Interlocked;

namespace Google.Protobuf
{
    /// <summary>
    /// A thread-safe object pool for Protobuf Messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class MessagePool<T> where T : class, new()
    {
        private static T _fastItem;
        private static Stack<T> _restItems;
        private static SpinLock _spinLock;
        private static int _maxSize = 10000;

        /// <summary>
        /// The max size of the pool.
        /// </summary>
        public static int MaxSize
        {
            get { return _maxSize; }
            set { Exchange(ref _maxSize, value); }
        }

        /// <summary>
        /// Gets an object from the pool.
        /// </summary>
        /// <returns></returns>
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

                    if (_restItems == null || _restItems.Count == 0)
                    {
                        item = new T();
                    }
                    else
                    {
                        item = _restItems.Pop();
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

        /// <summary>
        /// Releases an object to the pool.
        /// </summary>
        /// <param name="item"></param>
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
                if (_restItems.Count < _maxSize)
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
