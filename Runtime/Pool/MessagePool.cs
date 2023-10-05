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

        private static volatile T s_FastItem;
        private static volatile Node s_Head;
        private static volatile Node s_FreeList;
        private static volatile int s_NodeCount;

        public static T Get()
        {
            // Fast Path
            T item = Exchange(ref s_FastItem, null);

            // CAS s_Head
            if (item == null)
            {
                var spinWait = new SpinWait();

                while (true)
                {
                    Node node = s_Head;

                    if (node == null)
                    {
                        item = new T();
                        break;
                    }

                    if (CompareExchange(ref s_Head, node.Next, node) == node)
                    {
                        item = node.Item;
                        Decrement(ref s_NodeCount);

                        // Recycle Node（Only one try）
                        node.Item = null;
                        node.Next = s_FreeList;
                        CompareExchange(ref s_FreeList, node, node.Next);
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
            if (CompareExchange(ref s_FastItem, item, null) == null)
            {
                return;
            }

            var spinWait = new SpinWait();

            // Check Size
            while (true)
            {
                int count = s_NodeCount;

                if (count >= MessagePool.MaxSize)
                {
                    return;
                }

                if (CompareExchange(ref s_NodeCount, count + 1, count) == count)
                {
                    break;
                }

                spinWait.SpinOnce();
            }
            spinWait.Reset();

            // Get recycled Node (Only one try)
            Node node = s_FreeList;
            if (node is null || CompareExchange(ref s_FreeList, node.Next, node) != node)
            {
                node = new Node();
            }
            node.Item = item;

            // CAS s_Head
            while (true)
            {
                node.Next = s_Head;

                if (CompareExchange(ref s_Head, node, node.Next) == node.Next)
                {
                    break;
                }

                spinWait.SpinOnce();
            }
        }
    }
}
