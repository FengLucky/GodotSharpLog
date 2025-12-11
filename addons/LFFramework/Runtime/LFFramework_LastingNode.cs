using GDLog;
using Godot;

namespace LF;

public partial class LFFramework
{
    private static Node _root;

    public static void AddLastingNode(Node node,string name = null)
    {
        if (node.IsNotValid())
        {
            GLog.Warn("添加了一个无效的 node");
            return;
        }
        
        node.Name = name ?? node.GetType().Name;
        if (_root == null)
        {
            _root = new Node();
            _root.Name = nameof(LFFramework);
            (Engine.Singleton.GetMainLoop() as SceneTree)?.Root.CallDeferred("add_child",_root);
            _root.CallDeferred("add_child", node);
        }
        else
        {
            _root.AddChild(node);
        }
    }

    public static void Quit()
    {
        _root.GetTree().Quit();
    }
}