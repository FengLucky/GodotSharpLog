using GDLog;
using Godot;
namespace LF;

public partial class WindowAspectAdapter:Node
{
    private Window _window;

    private float _minAspect;
    private float _maxAspect;
    private float _defaultAspect;
    private bool _lockDefaultAspect;

    public WindowAspectAdapter(float minAspect, float maxAspect,bool lockDefaultAspect)
    {
        var defaultWidth = ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32();
        var defaultHeight = ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32();
        _defaultAspect = defaultWidth / (float)defaultHeight;
        _minAspect = minAspect;
        _maxAspect = maxAspect;
        _lockDefaultAspect = lockDefaultAspect;

        if (_maxAspect < _minAspect)
        {
            _maxAspect = float.MaxValue;
            GLog.Warn($"最大宽高比小于最小宽高比,该设置将被忽略");
        }

        if (_defaultAspect < _minAspect || _defaultAspect > _maxAspect)
        {
            _minAspect = float.MinValue;
            _maxAspect = float.MaxValue;
            GLog.Warn($"当前窗口的宽高比超出了设置的最小/最大宽高比范围,最小/最大宽高比将失效");
        }
    }
    
    public override void _EnterTree()
    {
        base._EnterTree();
        _window = GetWindow();
        _window.SizeChanged += OnWindowSizeChanged;
        OnWindowSizeChanged();
    }

    private void OnWindowSizeChanged()
    {
        var aspect = _window.Size.Aspect();
        var aspectMode = Window.ContentScaleAspectEnum.Expand;
        if (aspect < _minAspect || aspect > _maxAspect)
        {
            aspectMode = Window.ContentScaleAspectEnum.Keep;
        }

        if (aspectMode != _window.ContentScaleAspect)
        {
            _window.ContentScaleAspect = aspectMode;
            _window.ChildControlsChanged();
        }
    }
}