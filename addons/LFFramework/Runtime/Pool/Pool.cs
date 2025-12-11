using System;
using System.Runtime.CompilerServices;

namespace LF;

public class Pool<T>(Func<T> createFunc, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
    : BasePool<T>(createFunc, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize)
    where T : class, IPoolable
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new PoolItem<T> Get()
    {
        return new(this, base.Get());
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OnGet(T item)
    {
        item.OnGet();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OnRelease(T item)
    {
        item.Version++;
        item.OnRelease();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OnDestroy(T item)
    {
        item.OnDestroy();
    }
}