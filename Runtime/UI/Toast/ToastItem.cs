using Godot;

namespace LF;

public partial class ToastItem:Control
{
    [Export]
    private AnimationPlayer _animation;
    [Export]
    private Label _label;

    public void Show(string content)
    {
        _label.Text = content;
        _animation.Play("fly");
    }
}