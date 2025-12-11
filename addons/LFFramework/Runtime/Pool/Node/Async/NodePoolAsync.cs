using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Godot;

namespace LF;

public class NodePoolAsync<T>:BaseNodePoolAsync<T> where T : Node,IPoolable
{
    public NodePoolAsync(Func<CancellationToken, UniTask<T>> createFunc, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(createFunc, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
    }

    public NodePoolAsync(T template,  bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(template, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
    }

    public NodePoolAsync(string resPath, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000) : base(resPath, OnGet, OnRelease, OnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new async UniTask<PoolItem<T>> Get(CancellationToken token = default)
    {
        var item = await base.Get(token);
        return new(this,item);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OnGet(T node)
    {
        node.OnGet();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OnRelease(T node)
    {
        node.OnRelease();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OnDestroy(T node)
    {
        node.OnDestroy();
    }
}