namespace System.Runtime.CompilerServices
{
    internal static class Unsafe
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T Add<T>(ref T source, int elementOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern ref T AddByteOffset<T>(ref T source, IntPtr byteOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern T ReadUnaligned<T>(ref byte source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static extern void WriteUnaligned<T>(ref byte destination, T value);
    }
}
