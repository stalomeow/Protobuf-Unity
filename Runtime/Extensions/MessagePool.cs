using System.Threading;
using static System.Threading.Interlocked;

namespace Google.Protobuf
{
    /// <summary>
    /// A thread-safe and lock-free object pool for Protobuf Messages.
    /// </summary>
    public static class MessagePool
    {
        public static volatile int MaxSize = 100;
    }

    /// <summary>
    /// A thread-safe and lock-free object pool for Protobuf Messages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class MessagePool<T> where T : class, new()
    {
        private class Node
        {
            public T Item;
            public Node Next;
        }

        private static volatile T _fastItem;
        private static volatile Node _head;
        private static volatile Node _freeList;
        private static volatile int _nodeCount;

        public static T Get()
        {
            // Fast Path
            T item = Exchange(ref _fastItem, null);

            // CAS s_Head
            if (item == null)
            {
                var spinWait = new SpinWait();

                while (true)
                {
                    Node node = _head;

                    if (node == null)
                    {
                        item = new T();
                        break;
                    }

                    if (CompareExchange(ref _head, node.Next, node) == node)
                    {
                        item = node.Item;
                        Decrement(ref _nodeCount);

                        // Recycle Node（Only one try）
                        node.Item = null;
                        node.Next = _freeList;
                        CompareExchange(ref _freeList, node, node.Next);
                        break;
                    }

                    spinWait.SpinOnce();
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

            var spinWait = new SpinWait();

            // Check Size
            while (true)
            {
                int count = _nodeCount;

                if (count >= MessagePool.MaxSize)
                {
                    return;
                }

                if (CompareExchange(ref _nodeCount, count + 1, count) == count)
                {
                    break;
                }

                spinWait.SpinOnce();
            }
            spinWait.Reset();

            // Get recycled Node (Only one try)
            Node node = _freeList;
            if (node is null || CompareExchange(ref _freeList, node.Next, node) != node)
            {
                node = new Node();
            }
            node.Item = item;

            // CAS s_Head
            while (true)
            {
                node.Next = _head;

                if (CompareExchange(ref _head, node, node.Next) == node.Next)
                {
                    break;
                }

                spinWait.SpinOnce();
            }
        }
    }
}
