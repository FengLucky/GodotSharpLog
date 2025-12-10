using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GDLog;

public sealed class GLog
{
    private static LogLevel _logLevel = LogLevel.Info;
    private static readonly HashSet<ILogAgent> Agents = new();
    private static readonly HashSet<string> Categories = new();
    private static DebuggerLogAgent _debuggerLogAgent;
    private static readonly object Lock = new();

    public static void ChangeLogLevel(LogLevel level)
    {
        lock (Lock)
        {
            _logLevel = level;
        }
    }

    public static void AddAgent(ILogAgent agent)
    {
        if (agent == null)
        {
            return;
        }

        lock (Lock)
        {
            if (agent is DebuggerLogAgent debuggerLogAgent)
            {
                _debuggerLogAgent = debuggerLogAgent;
            }

            Agents.Add(agent);
            foreach (var category in Categories)
            {
                agent.AddCategory(category);
            }
        }
    }

    public static T GetAgent<T>() where T : class,ILogAgent
    {
        lock (Lock)
        {
            foreach (var agent in Agents)
            {
                if (agent.GetType().IsAssignableFrom(typeof(T)))
                {
                    return agent as T;
                }
            }
            
            return null;
        }
    }
    
    public static T[] GetAgents<T>() where T : class,ILogAgent
    {
        lock (Lock)
        {
            var agents = new List<T>();
            foreach (var agent in Agents)
            {
                if (agent.GetType().IsSubclassOf(typeof(T)))
                {
                    agents.Add(agent as T);
                }
            }

            return agents.ToArray();
        }
    }

    /// <summary>
    /// If generating the log content involves allocations (e.g. string concatenation, object creation),
    /// consider using this method to check whether the log level is enabled before actually building the message.
    /// This helps avoid unnecessary memory allocations (such as temporary strings) when logging is disabled.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool LogEnabled(LogLevel level)
    {
        return level >= _logLevel;
    }

    public static void Info(object message, string category = null)
    {
        if (message == null || !LogEnabled(LogLevel.Info))
        {
            return;
        }

        lock (Lock)
        {
            CollectCategory(category);
            foreach (var agent in Agents)
            {
                agent.Info(message, category);
            }
        }
    }

    public static void Warn(object message, string category = null)
    {
        if (message == null || !LogEnabled(LogLevel.Warn))
        {
            return;
        }

        lock (Lock)
        {
            CollectCategory(category);
            foreach (var agent in Agents)
            {
                agent.Warn(message, category);
            }
        }
    }

    public static void Error(object message, string category = null)
    {
        if (message == null || !LogEnabled(LogLevel.Error))
        {
            return;
        }

        lock (Lock)
        {
            CollectCategory(category);
            foreach (var agent in Agents)
            {
                agent.Error(message, category);
            }
        }
    }

    public static void Exception(Exception exception, string category = null)
    {
        if (exception == null || !LogEnabled(LogLevel.Exception))
        {
            return;
        }

        lock (Lock)
        {
            CollectCategory(category);
            foreach (var agent in Agents)
            {
                agent.Exception(exception, category);
            }
        }
    }

    [Conditional("DEBUG")]
    public static void DebugInfo(object message, string category = null)
    {
        if (message != null && _debuggerLogAgent != null)
        {
            lock (Lock)
            {
                CollectCategory(category);
                _debuggerLogAgent.Info(message, category);
            }
        }
    }
    
    [Conditional("DEBUG")]
    public static void DebugWarn(object message, string category = null)
    {
        if (message != null && _debuggerLogAgent != null)
        {
            lock (Lock)
            {
                CollectCategory(category);
                _debuggerLogAgent.Warn(message, category);
            }
        }
    }
    
    [Conditional("DEBUG")]
    public static void DebugError(object message, string category = null)
    {
        if (message != null && _debuggerLogAgent != null)
        {
            lock (Lock)
            {
                CollectCategory(category);
                _debuggerLogAgent.Error(message, category);
            }
        }
    }
    
    [Conditional("DEBUG")]
    public static void DebugException(Exception exception, string category = null)
    {
        if (exception != null && _debuggerLogAgent != null)
        {
            lock (Lock)
            {
                CollectCategory(category);
                _debuggerLogAgent.Exception(exception, category);
            }
        }
    }

    private static void CollectCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return;
        }
        
        if (Categories.Add(category))
        {
            foreach (var agent in Agents)
            {
                agent.AddCategory(category);
            }
        }
    }
}