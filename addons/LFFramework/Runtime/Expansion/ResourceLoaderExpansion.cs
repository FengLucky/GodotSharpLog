using System.Threading;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;

namespace LF;
public static class ResourceLoaderExpansion
{
    public static T Instantiate<T>(this ResourceLoaderInstance loader, string path,
        string typeHint = "", ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse) where T : Node
    {
        if (path.IsNotValidResource())
        {
            GLog.Error($"{path} 资源不存在");
            return null;
        }

        var resource = loader.Load(path, typeHint, cacheMode);
        if (resource is not PackedScene scene)
        {
            GLog.Error($"{path} 不是 {nameof(PackedScene)} 类型资源");
            return null;
        }
        
        return scene.Instantiate<T>();
    }
    
    public static async UniTask<T> LoadAsync<T>(this ResourceLoaderInstance loader,string path,
        string typeHint = "", bool useSubThreads = false, ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse,
        CancellationToken cancellationToken = default) where T:Resource
    {
        if (path.IsNotValidResource())
        {
            GLog.Error($"{path} 资源不存在");
            return null;
        }
        var error = loader.LoadThreadedRequest(path,typeHint,useSubThreads,cacheMode);
        if (error != Error.Ok)
        {
            return null;
        }

        ResourceLoader.ThreadLoadStatus status;
        do
        {
            await UniTask.Yield(cancellationToken);
            status = loader.LoadThreadedGetStatus(path);
        } while (status == ResourceLoader.ThreadLoadStatus.InProgress);
        
        if (status != ResourceLoader.ThreadLoadStatus.Loaded)
        {
            return null;
        }
        return loader.LoadThreadedGet(path) as T;
    }

    public static async UniTask<T> InstantiateAsync<T>(this ResourceLoaderInstance loader, string path,
        string typeHint = "", bool useSubThreads = false,
        ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Reuse,
        CancellationToken cancellationToken = default) where T : Node
    {
        var resource = await loader.LoadAsync<Resource>(path, typeHint, useSubThreads, cacheMode, cancellationToken);
        if (resource == null)
        {
            return null;
        }

        if (resource is not PackedScene scene)
        {
            GLog.Error($"{path} 不是 {nameof(PackedScene)} 类型资源");
            return null;
        }
        
        return scene.Instantiate<T>();
    }
}