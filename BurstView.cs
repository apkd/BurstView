using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using static System.Runtime.CompilerServices.MethodImplOptions;

static partial class BurstView
{
    [StructLayout(LayoutKind.Sequential)]
    struct ListHeader
    {
        readonly IntPtr data0, data1;
        public readonly Array Array;
    }

    [MethodImpl(AggressiveInlining)]
    public static unsafe T[] GetInternalArray<T>(List<T> list)
    {
        void* ptr = UnsafeUtility.PinGCObjectAndGetAddress(list, out ulong gcHandle);
        var array = UnsafeUtility.AsRef<ListHeader>(ptr).Array;
        UnsafeUtility.ReleaseGCObject(gcHandle);
        return array as T[] ?? Array.Empty<T>();
    }
}
