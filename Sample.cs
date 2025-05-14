using System;
using System.Threading.Tasks;
using Godot;
using GDLog;

public partial class Sample : Node
{
    public override async void _EnterTree()
    {
        base._EnterTree();
        await Task.Delay(10); // wait profiler start
        var fileLogAgent = new FileLogAgent();
        fileLogAgent.Cleanup(2); // cleanup log file
        GD.Print(fileLogAgent.GetCurrentLogPath());
        GLog.AddAgent(fileLogAgent);

        if (EngineDebugger.IsActive())
        {
            var debuggerLogAgent = new DebuggerLogAgent();
            GLog.AddAgent(debuggerLogAgent);
        }
        else
        {
            var godotLogAgent = new GodotLogAgent();
            GLog.AddAgent(godotLogAgent);
        }

        GLog.Info("info message", "Sample");
        GLog.Warn("warn message", "Sample");
        GLog.Error("error message", "Sample");
        GLog.Exception(new Exception("exception message"), "Sample");
    }
}