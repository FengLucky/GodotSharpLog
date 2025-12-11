using System;
using System.Runtime.CompilerServices;

namespace LF;

public record struct PoolItem<T>:IDisposable where T:class,IPoolable
{
    private readonly AbstractPool<T> _pool;
    private readonly int _version;
    private readonly WeakReference<T> _value;
    public PoolItem(AbstractPool<T> pool,T value)
    {
        _pool = pool;
        _version = value?.Version ?? -1;
        _value = new WeakReference<T>(value);
    }
    public T Value => _value.TryGetTarget(out var value) && value.Version == _version ? value : null;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Release()
    {
        if (_value.TryGetTarget(out var value) && value.Version == _version)
        {
            _pool.Release(value);
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() => Release();
}