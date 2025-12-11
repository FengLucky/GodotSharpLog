using System;
using System.Threading;
using Config;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;

namespace LF;

public partial class PageHolder
{
    public enum HolderState
    {
        New,
        Created,
        Closing,
        Closed,
    }

    /// <summary>
    /// 该 Holder 的根节点
    /// </summary>
    public Control Root { get; private set; }
    
    public PageBean Bean { get; private set; }
    
    public object Args { get; private set; }

    protected bool PageOpened { get; set; }
    public HolderState State { get; private set; } = HolderState.New;

    private UIPage _page;
    private bool _isValid;
    private CancellationTokenSource _cancellation = new();

    public virtual void SetArgs(object args)
    {
        Args = args;
        if (_page != null && PageOpened)
        {
            _page.RefreshArgs();
        }
    }

    public T GetArgs<T>()
    {
        try
        {
            return (T)Args;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public async UniTask<T> GetPage<T>(CancellationToken cancellationToken = default) where T : UIPage
    {
        if (State == HolderState.New)
        {
            GLog.Error($"界面 {Bean.Id} 未创建，无法获取界面");
            return null;
        }

        if (PageOpened)
        {
            return _page as T;
        }

        if (State >= HolderState.Closing)
        {
            return null;
        }
        
        var token = _cancellation.Token;
        while (!PageOpened)
        {
            token.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.Yield();
        }
        return _page as T;
    }
    
    public async UniTask WaitOpen(CancellationToken cancellationToken = default)
    {
        if (State == HolderState.New)
        {
            GLog.Error($"界面 {Bean.Id} 未创建无法等待打开");
            return;
        }

        if (State >= HolderState.Closing)
        {
            return;
        }

        var token = _cancellation.Token;
        while (!PageOpened)
        {
            token.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.Yield();
        }
    }

    public async UniTask WaitClose(CancellationToken cancellationToken = default)
    {
        if (State == HolderState.New)
        {
            GLog.Error($"界面 {Bean.Id} 未创建，无法等待关闭");
            return;
        }
        
        var token = _cancellation.Token;
        while (State != HolderState.Closed)
        {
            token.ThrowIfCancellationRequested();
            cancellationToken.ThrowIfCancellationRequested();
            await UniTask.Yield();
        }
    }

    /// <summary>
    /// 在 window 中创建
    /// </summary>
    /// <param name="window"></param>
    /// <param name="root"></param>
    public bool Create(Control root)
    {
        if (State >= HolderState.Created)
        {
            GLog.Warn($"请勿手动调用 {nameof(PageHolder)} 的 {nameof(Create)} 方法");
            return false;
        }
        
        if (!_isValid)
        {
            GLog.Error($"{GetType().Name} 必须使用 {nameof(PageHolder)}.{nameof(Allocate)} 分配，不可以自己 new()");
            return false;
        }
        
        if (!CanOpen())
        {
            return false;
        }
        
        Root = root;
        State = HolderState.Created;
        LoadPage().Forget();
        return true;
    }

    public void Close()
    {
        CloseAsync().Forget();
    }

    public UniTask CloseAsync()
    {
        return PageManager.Instance.CloseAsync(this);
    }

    public async UniTask CloseHandle()
    {
        if (State >= HolderState.Closing)
        {
            return;
        }
        State = HolderState.Closing;
        if (Holders.TryGetValue(Bean.Id, out var set))
        {
            set.Remove(this);
        }

        await _cancellation.CancelAsync();
        if (_page != null)
        {
            await _page.CloseAnimation();
            _page.CloseHandle();
            _page.QueueFree();
        }
        _page = null;
        OnClose();
        Root.QueueFree();
        Root = null;
        PageOpened = false;
        State = HolderState.Closed;
    }

    #region 生命周期
    
    /// <summary>
    /// 界面是否可以打开
    /// </summary>
    /// <returns></returns>
    protected virtual bool CanOpen()
    {
        return true;
    }
    
    public virtual void OnClose()
    {
    }

    #endregion
    
    private async UniTaskVoid LoadPage()
    {
        var token = _cancellation.Token;
        if (!PageOpened)
        {
            var loadResult = await ResourceLoader.Singleton.LoadAsync<PackedScene>(Bean.ResPath, cancellationToken: token)
                .SuppressCancellationThrow();
            if (loadResult.IsCanceled || loadResult.Result == null)
            {
                await CloseAsync();
                return;
            }

            var page = loadResult.Result.Instantiate<UIPage>();
            if (page == null)
            {
                GLog.Error($"{Bean.ResPath} 不是 {nameof(UIPage)}");
                await CloseAsync();
                return;
            }
            _page = page;
            _page.Create(this);
            var canceled = await _page.Preload(token).SuppressCancellationThrow();
            if (canceled)
            {
                await CloseAsync();
                return;
            }
        }
        
        Root.AddChild(_page);
        _page.Open();
        PageOpened = true;
    }
}