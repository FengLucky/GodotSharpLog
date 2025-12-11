using System;
using System.Collections.Generic;
using Config;
using Cysharp.Threading.Tasks;
using Godot;

namespace LF;
public class PageManager : Manager<PageManager>
{
    public event Action<PageHolder> OnPageOpened;
    public event Action<PageHolder> OnPageClosed;
    public event Action<PageHolder> OnCurrentUIPageChanged;
    public static Control PageRoot { get; private set; }
    
    private readonly Dictionary<PageLayer, Control> _layers = new();
    private readonly Dictionary<PageLayer, List<PageHolder>> _layerHolders = new();
    private PageHolder _currentUIPage;
    
    protected override void OnInit()
    {
        base.OnInit();
        var pageRoot = new Control();
        pageRoot.Name = "PageRoot";
        pageRoot.FullRect();
        pageRoot.ProcessMode = Node.ProcessModeEnum.Always;
        pageRoot.MouseFilter = Control.MouseFilterEnum.Ignore;
        for (var i = 0; i < (int)PageLayer.Count; i++)
        {
            var layer = new Control();
            layer.Name = $"{(PageLayer)i}Layer";
            layer.FullRect();
            layer.MouseFilter = Control.MouseFilterEnum.Ignore;
            pageRoot.AddChild(layer);
            _layers.Add((PageLayer)i,layer);
            _layerHolders.Add((PageLayer)i,new());   
        }
        (Engine.Singleton.GetMainLoop() as SceneTree)?.Root.CallDeferred("add_child",pageRoot);
    }

    public void Open(PageHolder holder)
    {
        var holders = _layerHolders[holder.Bean.Layer];
        var index = holders.IndexOf(holder);
        if (index >= 0)
        {
            holders.Remove(holder);
        }
        else
        {
            var root = new Control();
            root.Name = $"{holder.Bean.ResPath.Replace("res://", "")}";
            root.FullRect();
            root.MouseFilter = Control.MouseFilterEnum.Ignore;
            if (!holder.Create(root))
            {
                root.QueueFree();
                return;
            }
            _layers[holder.Bean.Layer].AddChild(root);
        }
        
        holder.Root.MoveToFront();
        holders.Add(holder);
        WaitOpen(holder).Forget();
    }

    public PageHolder Open(int id, object args = null)
    {
        var holder = PageHolder.Allocate(id);
        if (holder == null)
        {
            return null;
        }

        holder.SetArgs(args);
        Open(holder);
        return holder;
    }

    public void Close(PageHolder holder)
    {
        CloseAsync(holder).Forget();
    }

    public async UniTask CloseAsync(PageHolder holder)
    {
        if (holder.State >= PageHolder.HolderState.Closing)
        {
            return;
        }

        if (holder.State == PageHolder.HolderState.Closing)
        {
            await holder.WaitClose();
            return;
        }
        
        var holders = _layerHolders[holder.Bean.Layer];
        holders.Remove(holder);
        await holder.CloseHandle();
        RefreshHolderLayer(holder.Bean.Layer);
        OnPageClosed.SafeInvoke(holder);
    }

    public UniTask CloseAll(PageLayer layer)
    {
        var holders = new List<PageHolder>();
        holders.AddRange(_layerHolders[layer]);
        var tasks = new List<UniTask>();
        foreach (var holder in holders)
        {
            tasks.Add(holder.CloseAsync());
        }

        return UniTask.WhenAll(tasks);
    }

    public async UniTask SetTheme(string themePath)
    {
        PageRoot.Theme = await ResourceLoader.Singleton.LoadAsync<Theme>(themePath);
    }

    private async UniTaskVoid WaitOpen(PageHolder holder)
    {
        var canceled = await holder.WaitOpen().SuppressCancellationThrow();
        if (!canceled)
        {
            RefreshHolderLayer(holder.Bean.Layer);
            OnPageOpened.SafeInvoke(holder);
        }
    }

    private void RefreshHolderLayer(PageLayer layer)
    {
        var holders = _layerHolders[layer];
        // for (int i = 0; i < holders.Count; i++)
        // {
        //     var holder = holders[i];
        //     if (i == 0)
        //     {
        //         holder.Root.ZIndex = 0;
        //     }
        //     else
        //     {
        //         var lastHolder = holders[i - 1];
        //         holder.Root.ZIndex = lastHolder.Root.ZIndex + lastHolder.Bean.ZThickness;
        //     }
        // }

        var hasCover = false;
        var hasTransparent = false;
        PageHolder lastPage = null;
        for (int i = holders.Count - 1; i >= 0; i--)
        {
            var holder = holders[i];
            lastPage ??= holder;
            holder.Root.Visible = !hasTransparent;
            switch (holder.Bean.Type)
            {
                case PageType.Cover:
                    if (!hasTransparent)
                    {
                        hasCover = true;
                    }
                    break;
                case PageType.Transparent:
                    if (!hasCover)
                    {
                        hasTransparent = true;
                    }
                    break;
            }
        }

        if (layer == PageLayer.UI && _currentUIPage != lastPage)
        {
            _currentUIPage = lastPage;
            OnCurrentUIPageChanged.SafeInvoke(lastPage);
        }
    }
}