using Godot;

namespace LF;

public static class ControlExpansion
{
    public static void FullRect(this Control control)
    {
        control.AnchorLeft = 0.0f;   // 左锚点
        control.AnchorTop = 0.0f;    // 上锚点
        control.AnchorRight = 1.0f;  // 右锚点
        control.AnchorBottom = 1.0f; // 下锚点
        //control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
    }
}