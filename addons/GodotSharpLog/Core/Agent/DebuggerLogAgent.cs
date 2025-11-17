using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot;
using Array = Godot.Collections.Array;

namespace GDLog;

public partial class GLogProfiler : EngineProfiler
{
    private Array _messages = new();
    private Array _categories = new();
    public bool Enabled { get; private set; }

    public override void _Toggle(bool enable, Array options)
    {
        base._Toggle(enable, options);
        Enabled = enable;
        if (!enable)
        {
            lock (_messages)
            {
                _messages.Clear();
            }
        }
    }

    public override void _Tick(double frameTime, double processTime, double physicsTime, double physicsFrameTime)
    {
        base._Tick(frameTime, processTime, physicsTime, physicsFrameTime);

        lock (_messages)
        {
            if (_messages.Count > 0)
            {
                EngineDebugger.SendMessage("gd_log:message",  _messages);
                _messages = new();
            }

            if (_categories.Count > 0)
            {
                EngineDebugger.SendMessage("gd_log:category",  _categories);
                _categories = new();
            }
        }
    }

    public void AddMessage(LogLevel level, string message, string category,string stackTrace)
    {
        lock (_messages)
        {
            _messages.Add((int)level);
            _messages.Add(category);
            _messages.Add(message);
            _messages.Add(stackTrace);
        }
    }
    
    public void AddCategory(string category)
    {
        lock (_messages)
        {
            _categories.Add(category);
        }
    }
}

public class DebuggerLogAgent:ILogAgent
{
    private static bool _initialized;
    private static GLogProfiler _profiler;
    
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    internal static void Init()
    {
        if (EngineDebugger.IsActive() && !_initialized)
        {
            _initialized = true;
            _profiler = new();
            EngineDebugger.RegisterProfiler("gd_log", _profiler);
            EngineDebugger.ProfilerEnable("gd_log",true);
        }
    }
    
    public void AddCategory(string category)
    {
        if (_profiler?.Enabled == true)
        {
            _profiler.AddCategory(category);
        }
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

    private void Log(LogLevel level, object message, string category)
    {
        if (_profiler?.Enabled != true)
        {
            return;
        }
        
        _profiler.AddMessage(level,message.ToString(),category,new StackTrace(3,true).Format(false,true));
    }
}