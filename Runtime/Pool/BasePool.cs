using System;
using System.Runtime.CompilerServices;

namespace LF;

public class BasePool<T>(
    Func<T> createFunc,
    Action<T> actionOnGet = null,
    Action<T> actionOnRelease = null,
    Action<T> actionOnDestroy = null,
    bool collectionCheck = true,
    int defaultCapacity = 10,
    int maxSize = 10000)
    : AbstractPool<T>(actionOnGet,
        actionOnRelease,
        actionOnDestroy,
        collectionCheck,
        defaultCapacity,
        maxSize)
    where T : class
{
    private readonly Func<T> _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual T Get()
    {
        if (!TryGetPooled(out var item))
        {
            item = _createFunc.SafeInvoke();
        }

        ActionOnGet.SafeInvoke(item);
        return item;
    }
}