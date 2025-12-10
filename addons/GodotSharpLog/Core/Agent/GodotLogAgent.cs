using System;
using Godot;

namespace GDLog;

public class GodotLogAgent() : ILogAgent
{
    public void AddCategory(string category)
    {
        
    }

    public void Info(object message, string category)
    {
        Log(LogLevel.Info,message,category);
    }

    public void Warn(object message, string category)
    {
        Log(LogLevel.Warn,message,category);
    }

    public void Error(object message, string category)
    {
        Log(LogLevel.Error,message,category);
    }

    public void Exception(Exception exception, string category)
    {
        Log(LogLevel.Exception,exception.Message,category);
    }

    private void Log(LogLevel level, object message, string category)
    {
        string content = !string.IsNullOrWhiteSpace(category) ? $"[{category}]" : string.Empty;
        content += message;

        switch (level)
        {
            case LogLevel.Info:
                GD.Print(content);
                break;
            case LogLevel.Warn:
                GD.PushWarning(content);
                break;
            case LogLevel.Error:
            case LogLevel.Exception:
                GD.PushError(content);
                break;
        }
    }
}