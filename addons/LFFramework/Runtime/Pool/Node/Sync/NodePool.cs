using System;
using System.Runtime.CompilerServices;
using Godot;

namespace LF;

public class NodePool<T>:BaseNodePool<T> where T : Node, IPoolable
{
    public NodePool(Func<T> createFunc, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(createFunc, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
    }

    public NodePool(T template,  bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(template, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
    }

    public NodePool(string resPath, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(resPath, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
    }
    
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