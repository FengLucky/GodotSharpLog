using Config;
using Cysharp.Threading.Tasks;

namespace LF;

public static class Loading
{
    public static async UniTask<LoadingPage> Show(int pageId = PageTableConst.BlackLoading)
    {
        var hodler = PageManager.Instance.Open(pageId);
        if (hodler == null)
        {
            return null;
        }

        var page = await hodler.GetPage<LoadingPage>();
        if (page != null)
        {
            await page.WaitAnimationPlayFinished();
        }

        return page;
    }
}