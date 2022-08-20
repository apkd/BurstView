using System;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

public static partial class BurstView
{
    public struct ViewHandle : IDisposable
    {
        readonly ulong m_GCHandle;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
        DisposeSentinel m_DisposeSentinel;
#endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        public ViewHandle(ulong gcHandle, AtomicSafetyHandle safety, DisposeSentinel disposeSentinel)
        {
            m_Safety = safety;
            m_DisposeSentinel = disposeSentinel;
            m_GCHandle = gcHandle;
        }
#else
        public ViewHandle(ulong gcHandle)
            => m_GCHandle = gcHandle;
#endif

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
            UnsafeUtility.ReleaseGCObject(m_GCHandle);
        }

        public JobHandle Dispose(JobHandle dependsOn)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Clear(ref m_DisposeSentinel);

            var jobHandle = new ViewHandleDisposeJob
                {
                    Data = new ViewHandleDispose(m_GCHandle, m_Safety),
                }
                .Schedule(dependsOn);

            AtomicSafetyHandle.Release(m_Safety);
            return jobHandle;
#else
            return new ViewHandleDisposeJob
            {
                Data = new ViewHandleDispose(m_GCHandle),
            }
            .Schedule(dependsOn);
#endif
        }

        [NativeContainer]
        struct ViewHandleDispose
        {
            readonly ulong m_GCHandle;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle m_Safety;
#endif

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            public ViewHandleDispose(ulong gcHandle, AtomicSafetyHandle safety)
            {
                m_GCHandle = gcHandle;
                m_Safety = safety;
            }
#else
            public ViewHandleDispose(ulong gcHandle)
                => m_GCHandle = gcHandle;
#endif

            public void Dispose()
                => UnsafeUtility.ReleaseGCObject(m_GCHandle);
        }

        [BurstCompile]
        struct ViewHandleDisposeJob : IJob
        {
            public ViewHandleDispose Data;

            public void Execute()
                => Data.Dispose();
        }
    }
}
