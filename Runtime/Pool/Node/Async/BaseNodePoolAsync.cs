using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;

namespace LF;

public class BaseNodePoolAsync<T> : AbstractPool<T> where T : Node
{
    private readonly Func<CancellationToken,UniTask<T>> _createFunc;
    private readonly T _template;
    private readonly string _resPath;

    public BaseNodePoolAsync(Func<CancellationToken,UniTask<T>> createFunc,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
        : base(actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize)
    {
        _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
    }

    public BaseNodePoolAsync(T template,
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

    public BaseNodePoolAsync(string resPath,
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
    public virtual async UniTask<T> Get(CancellationToken token= default)
    {
        if (!TryGetPooled(out var item))
        {
            if (_createFunc != null)
            {
                item = await _createFunc.SafeInvoke(token);
            }
            else if (_template != null)
            {
                item = _template.Duplicate() as T;
            }
            else
            {
                item = await ResourceLoader.Singleton.InstantiateAsync<T>(_resPath, cancellationToken: token);
            }
        }

        ActionOnGet.SafeInvoke(item);
        return item;
    }
}