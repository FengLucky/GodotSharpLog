using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace GDLog;

public class BuiltinLogAgent(uint historyCount = 1000) : ILogAgent
{
    private readonly Queue<Variant> _histories = new();
    private readonly Array<string> _categories = new();
    private Control _panel;
    
    public bool PanelOpened { get; private set; }

    public void AddCategory(string category)
    {
        _categories.Add(category);
        _panel?.Call("add_category", category);
    }

    public void Info(object message, string category)
    {
        Log(LogLevel.Info, message, category);
    }

    public void Warn(object message, string category)
    {
        Log(LogLevel.Warn, message, category);
    }

    public void Error(object message, string category)
    {
        Log(LogLevel.Error, message, category);
    }

    public void Exception(Exception exception, string category)
    {
        Log(LogLevel.Exception, exception.Message, category);
    }

    public void OpenLogPanel(Control parent = null)
    {
        var res = ResourceLoader.Load<PackedScene>("res://addons/GodotSharpLog/Resources/LogPanel.tscn");
        _panel = res.Instantiate<Control>();
        _panel.Call("init", 0);
        _panel.Call("add_categories", _categories);
        _panel.Call("add_logs", new Array<Variant>(_histories));
        _panel.Connect("close", Callable.From(OnPanelClosed));
        if (parent != null)
        {
            parent.AddChild(_panel);
        }
        else
        {
            if (Engine.GetMainLoop() is SceneTree sceneTree)
            {
                _panel.Call("show_close");
                sceneTree.Root.AddChild(_panel);
            }
        }

        PanelOpened = true;
        res.Dispose();
    }

    public void CloseLogPanel()
    {
        _panel?.QueueFree();
        _panel = null;
        PanelOpened = false;
    }

    private void Log(LogLevel level, object message, string category)
    {
        var stack = new StackTrace(3, true).Format(false);
        _histories.Enqueue((int)level);
        _histories.Enqueue(category);
        _histories.Enqueue(message.ToString());
        _histories.Enqueue(stack);
        
        if (_histories.Count > historyCount * 4)
        {
            _histories.Dequeue();
            _histories.Dequeue();
            _histories.Dequeue();
            _histories.Dequeue();
        }

        _panel?.Call("add_log", (int)level, category, message.ToString(), stack);
    }

    private void OnPanelClosed()
    {
        _panel = null;
        PanelOpened = false;
    }
}