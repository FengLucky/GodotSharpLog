using Godot;
using Godot.Collections;

namespace LF;

public partial class ToastPage:UIPage
{
    [Export]
    private Array<ToastItem> _items;

    private int _index;
    public void Show(string content)
    {
        var item = _items[_index];
        item.Show(content);
        _index++;
        _index %= _items.Count;
    }
}