using Config;
using Cysharp.Threading.Tasks;
using GDLog;

namespace LF;

public static partial class ConfirmDialog
{
    public static UniTask<ConfirmDialogResult> Show(string title, string message,string okText = "确定",string cancelText = "取消",bool hasCancel = true)
    {
        return Show(PageTableConst.ConfirmDialog, title, message, okText,cancelText,hasCancel);
    }
    
    public static UniTask<ConfirmDialogResult> Show(int pageId ,string title, string message,string okText = "确定",string cancelText = "取消", bool hasCancel = true)
    {
        var args = new ConfirmDialogArgs
        {
            Title = title,
            Content = message,
            HasCancel = hasCancel,
            OkText = okText,
            CancelText = cancelText
        };

        return Show(pageId, args);
    }

    public static async UniTask<ConfirmDialogResult> Show(int pageId,ConfirmDialogArgs args)
    {
        var holder = PageManager.Instance.Open(pageId, args);
        if (holder == null)
        {
            GLog.Error($"确认对话框界面不存在:{pageId}");
            return ConfirmDialogResult.Cancel;
        }

        await holder.WaitClose();
        return args.Result;
    }
}