using System;
using Cysharp.Threading.Tasks;

namespace LF;

public static class UniTaskExpansion
{
    public static void OnCompleted(this UniTask task, Action callback)
    {
        task.GetAwaiter().OnCompleted(() =>
        {
            if (task.Status == UniTaskStatus.Succeeded)
            {
                callback.SafeInvoke();
            }
        });
    }
    
    public static void OnCompleted(this UniTask task, Action<UniTaskStatus> callback)
    {
        task.GetAwaiter().OnCompleted(() =>
        {
            callback.SafeInvoke(task.Status);
        });
    }
    
    public static void OnCompleted<T>(this UniTask<T> task, Action<T> callback)
    {
        var awaiter = task.GetAwaiter();
        task.GetAwaiter().OnCompleted(() =>
        {
            if (task.Status == UniTaskStatus.Succeeded)
            {
                callback.SafeInvoke(awaiter.GetResult());
            }
        });
    }
    
    public static void OnCompleted<T>(this UniTask<T> task, Action<UniTaskStatus,T> callback)
    {
        var awaiter = task.GetAwaiter();
        task.GetAwaiter().OnCompleted(() =>
        {
            callback.SafeInvoke(task.Status,awaiter.GetResult());
        });
    }
}