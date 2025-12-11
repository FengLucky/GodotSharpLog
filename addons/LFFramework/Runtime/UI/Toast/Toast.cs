using Config;
using Cysharp.Threading.Tasks;

namespace LF;

public static class Toast
{
    private static ToastPage _toastPage;
    public static void Show(string content)
    {
        if (_toastPage != null)
        {
            _toastPage.Show(content);
            return;
        }
        
        WaitOpenAndShow(content).Forget();
    }

    private static async UniTaskVoid WaitOpenAndShow(string content)
    {
        var holder = PageManager.Instance.Open(PageTableConst.Toast);
        var page = await holder.GetPage<ToastPage>();
        page.Show(content);
        _toastPage = page;
    }
}