using UnityEngine.Scripting;

namespace System.Runtime.CompilerServices
{
    internal static class Unsafe
    {
        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T Add<T>(ref T source, int elementOffset);

        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T AddByteOffset<T>(ref T source, IntPtr byteOffset);

        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T ReadUnaligned<T>(ref byte source);

        [Preserve]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void WriteUnaligned<T>(ref byte destination, T value);
    }
}
