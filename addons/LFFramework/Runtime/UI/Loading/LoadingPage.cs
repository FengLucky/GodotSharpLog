using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;

namespace LF;

public partial class LoadingPage:UIPage
{
    [Export]
    protected ProgressBar ProgressBar;
    protected double Progress;
    private bool _animationPlayFinished;
    private bool _closed;

    public async UniTask WaitAnimationPlayFinished()
    {
        while (!_animationPlayFinished && !_closed)
        {
            await UniTask.Yield();
        }
    }
    
    public void SetProgress(double progress)
    {
        Progress = progress;
        if (ProgressBar.IsValid())
        {
            ProgressBar.Value = progress;
        }
        
        try
        {
            OnProgressChanged();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    protected override async UniTask OnShowAnimation(CancellationToken cancellationToken = default)
    {
        await base.OnShowAnimation(cancellationToken);
        _animationPlayFinished = true;
    }

    protected virtual void OnProgressChanged()
    {
        
    }

    protected override void OnClose()
    {
        base.OnClose();
        _closed = true;
    }
}