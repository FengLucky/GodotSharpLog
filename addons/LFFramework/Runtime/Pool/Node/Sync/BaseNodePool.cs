using System;
using System.Runtime.CompilerServices;
using GDLog;
using Godot;

namespace LF;

public class BaseNodePool<T>:AbstractPool<T> where T:Node
{
    private readonly Func<T> _createFunc;
    private readonly T _template;
    private readonly string _resPath;
    
    public BaseNodePool(Func<T> createFunc,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
        :base(actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
        _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
    }

    public BaseNodePool(T template,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
        : base(actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
        if (template == null)
        {
            GLog.Error($"{nameof(BaseNodePool<T>)} 模板为空");
        }
        _template = template;
    }

    public BaseNodePool(string resPath,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
        : base(actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
        if (resPath.IsNullOrWhiteSpace() || !FileAccess.FileExists(resPath))
        {
            GLog.Error($"{nameof(BaseNodePool<T>)} 资源路径不存在:{resPath}");
        }

        _resPath = resPath;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual T Get()
    {
        if (!TryGetPooled(out var item))
        {
            if (_createFunc != null)
            {
                item = _createFunc.SafeInvoke();
            }
            else if (_template != null)
            {
                item = _template.Duplicate() as T;
            }
            else
            {
                item = ResourceLoader.Singleton.Instantiate<T>(_resPath);
            }
        }

        ActionOnGet.SafeInvoke(item);
        return item;
    }
}