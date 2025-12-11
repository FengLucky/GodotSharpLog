using Godot;

namespace LF;

public enum ConfirmDialogResult
{
    Ok,
    Cancel
}
public class ConfirmDialogArgs
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string OkText { get; set; }
    public string CancelText { get; set; }
    public bool HasCancel { get; set; }
    public ConfirmDialogResult Result { get; set; } = ConfirmDialogResult.Cancel;
}
public partial class ConfirmDialogPage:UIPage<ConfirmDialogArgs>
{
    [Export]
    protected Button OkButton;
    [Export]
    protected Button CancelButton;
    [Export]
    protected RichTextLabel Title;
    [Export]
    protected RichTextLabel Content;

    public override void _Ready()
    {
        base._Ready();
        OkButton.Pressed += OnClickOk;
        OkButton.Text = Args.OkText;
        Title.Text = Args.Title;
        Content.Text = Args.Content;
        if (CancelButton.IsValid())
        {
            CancelButton.Pressed += OnClickCancel;
            CancelButton.Text = Args.CancelText;
            CancelButton.Visible = Args.HasCancel;
        }
    }

    private void OnClickOk()
    {
        Args.Result = ConfirmDialogResult.Ok;
        Close();
    }
    
    private void OnClickCancel()
    {
        Args.Result = ConfirmDialogResult.Cancel;
        Close();
    }
}