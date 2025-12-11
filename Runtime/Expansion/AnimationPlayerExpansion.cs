using System.Threading;
using Cysharp.Threading.Tasks;
using Godot;

namespace LF;

public static class AnimationPlayerExpansion
{
    public static async UniTask PlayAndWait(this AnimationPlayer animation,string animationName,CancellationToken cancellation = default)
    {
         animation.Play(animationName);
         while (animation.IsPlaying())
         {
             await UniTask.Yield(cancellation);
         }
    }
}