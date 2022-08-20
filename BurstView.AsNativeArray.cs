using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static partial class BurstView
{
    /// <summary>
    /// View <paramref name="array"/> as a <see cref="NativeArray{T}"/> without having to copy it or doing all the boilerplate for getting a pointer to the array. 
    /// Useful for allowing a job to work on an array.
    ///
    /// <para>
    /// You do not need to dispose the <paramref name="nativeArray"/>, but you need to dispose the <see cref="ViewHandle"/> you get back, Unity's Memory Leak Detection will tell you if you forget.
    /// Do not use the <paramref name="nativeArray"/> after calling <see cref="ViewHandle.Dispose"/> on the <see cref="ViewHandle"/> returned from this function, 
    /// as you can risk the garbage collector removing the data from down under you, Unity's Collections Safety Checks will tell you if you do this.
    /// There is <b>no</b> race detection for accessing multiple different views of the same array in different jobs concurrently.
    /// </para>
    /// 
    /// Usage:
    /// <code>
    /// int[] array;
    /// using (array.ViewAsNativeArray(out var nativeArray))
    /// {
    ///     // work on nativeArray
    /// }
    /// </code>
    /// </summary>
    public static unsafe ViewHandle ViewAsNativeArray<T>(this T[] array, out NativeArray<T> nativeArray) 
        where T : struct
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (array is null)
            throw new ArgumentNullException(nameof(array));
#endif

        var ptr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);
        nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, array.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out var safety, out var sentinel, callSiteStackDepth: 0, Allocator.None);
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, safety);
        return new ViewHandle(handle, safety, sentinel);
#else
        return new ViewHandle(handle);
#endif
    }

    /// <summary>
    /// View <paramref name="list"/> as a <see cref="NativeArray{T}"/> without having to copy it or doing all the boilerplate for getting the pointer out of a list. 
    /// Useful for allowing a job to work on a list.
    /// 
    /// <para>
    /// Put this thing in a disposable scope unless you can guarantee that the list will never change size or reallocate (in that case consider using a <see cref="NativeArray{T}"/> instead),
    /// as Unity will <b>not</b> tell you if you're out of bounds, accessing invalid data, or accessing stale data because you have a stale/invalid view of the list.
    /// The following changes to the list will turn a view invalid/stale:
    /// <list type="number">
    /// <item>The contents of the array will be stale (not reflect any changes to the values in the list) in case of a reallocation (changes to, or adding more items than, <see cref="List{T}.Capacity"/> or using <see cref="List{T}.TrimExcess"/>)</item>
    /// <item>The length of the array will be wrong if you add/remove elements from the list</item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// The <paramref name="nativeArray"/> itself does not need to be disposed, but you need to dispose the <see cref="ViewHandle"/> you get back, Unity's Memory Leak Detection will tell you if you forget.
    /// Do not use the array after calling <see cref="ViewHandle.Dispose"/> on the <see cref="ViewHandle"/> returned from this function, 
    /// as you can risk the garbage collector removing the data from down under you, Unity's Collections Safety Checks will tell you if you do this.
    /// There is <b>no</b> race detection for accessing multiple different views of the same list in different jobs concurrently, or modifying the list while a job is working on a view.
    /// </para>
    /// 
    /// Usage:
    /// <code>
    /// List&lt;int&gt; list;
    /// using (list.ViewAsNativeArray(out var nativeArray))
    /// {
    ///     // work on nativeArray
    /// }
    /// </code>
    /// </summary>
    public static unsafe ViewHandle ViewAsNativeArray<T>(this List<T> list, out NativeArray<T> nativeArray) 
        where T : struct
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        if (list is null)
            throw new ArgumentNullException(nameof(list));
#endif

        var array = GetInternalArray(list);
        var ptr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);
        nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, list.Count, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Create(out var safety, out var sentinel, callSiteStackDepth: 0, Allocator.None);
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, safety);
        return new ViewHandle(handle, safety, sentinel);
#else
        return new ViewHandle(handle);
#endif
    }
}
