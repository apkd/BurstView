using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

public static partial class BurstView
{
    public static unsafe ViewHandle ViewAsUnsafeList<T>(this T[] array, out UnsafeList<T> unsafeList) 
        where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (array is null)
            throw new ArgumentNullException(nameof(array));
#endif
        
        var ptr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);
        unsafeList = new UnsafeList<T>(ptr: (T*)ptr, length: array.Length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out var safety, out var sentinel, callSiteStackDepth: 0, Allocator.None);
        return new ViewHandle(handle, safety, sentinel);
#else
        return new ViewHandle(handle);
#endif
    }

    public static unsafe ViewHandle ViewAsUnsafeList<T1, T2>(this T1[] array, out UnsafeList<T2> unsafeList)
        where T1 : unmanaged
        where T2 : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        Assert.AreEqual(sizeof(T1), sizeof(T2));
#endif

        var ptr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);
        unsafeList = new UnsafeList<T2>(ptr: (T2*)ptr, length: array.Length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out var safety, out var sentinel, callSiteStackDepth: 0, Allocator.None);
        return new ViewHandle(handle, safety, sentinel);
#else
        return new ViewHandle(handle);
#endif
    }

    public static unsafe ViewHandle ViewAsUnsafeList<T>(this List<T> list, out UnsafeList<T> nativeArray) 
        where T : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (list is null)
            throw new ArgumentNullException(nameof(list));
#endif

        var array = GetInternalArray(list);
        var ptr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);
        nativeArray = new UnsafeList<T>(ptr: (T*)ptr, length: list.Count);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out var safety, out var sentinel, callSiteStackDepth: 0, Allocator.None);
        return new ViewHandle(handle, safety, sentinel);
#else
        return new ViewHandle(handle);
#endif
    }

    public static unsafe ViewHandle ViewAsUnsafeList<T1, T2>(this List<T1> list, out UnsafeList<T2> nativeArray)
        where T1 : unmanaged
        where T2 : unmanaged
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (list is null)
            throw new ArgumentNullException(nameof(list));

        Assert.AreEqual(sizeof(T1), sizeof(T2));
#endif

        var array = GetInternalArray(list);
        var ptr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);
        nativeArray = new UnsafeList<T2>(ptr: (T2*)ptr, length: list.Count);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out var safety, out var sentinel, callSiteStackDepth: 0, Allocator.None);
        return new ViewHandle(handle, safety, sentinel);
#else
        return new ViewHandle(handle);
#endif
    }
    
    public static unsafe UnsafeList<T> AsUnsafeList<T>(this in NativeArray<T> array) where T : unmanaged
        => new(ptr: (T*) array.GetUnsafePtr(), length: array.Length);
    
    public static unsafe UnsafeList<T> AsReadOnlyUnsafeList<T>(this in NativeArray<T> array) where T : unmanaged
        => new(ptr: (T*) array.GetUnsafeReadOnlyPtr(), length: array.Length);
}
