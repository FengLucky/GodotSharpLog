using System;
using System.Runtime.CompilerServices;

namespace LF;

public class CollectionPoolItem<T>(BasePool<T> pool) : IDisposable
    where T : class
{
    private T _value;
    public T Value => _value ??= pool.Get();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Release()
    {
        pool.Release(_value);
        _value = null;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Release();
}