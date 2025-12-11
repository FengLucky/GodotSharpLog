using System.Runtime.CompilerServices;
using Godot;

namespace LF;
public static class CanvasItemExpansion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InvertVisible(this CanvasItem item)
    {
        if (item?.IsValid() == true)
        {
            item.Visible = !item.Visible;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeSetVisible(this CanvasItem item, bool active)
    {
        if (item?.IsValid() == true)
        {
            item.Visible = active;
        }
    }
}