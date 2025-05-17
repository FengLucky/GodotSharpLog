using System;
using System.Threading.Tasks;
using Godot;
using GDLog;

public partial class Sample : Node
{
    private double _spaceInterval = 0;
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
        
        var builtinLogAgent = new BuiltinLogAgent();
        GLog.AddAgent(builtinLogAgent);

        GLog.Info("info message", "Sample");
        GLog.Warn("warn message", "Sample");
        GLog.Error("error message", "Sample");
        GLog.Exception(new Exception("exception message"), "Sample");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_spaceInterval > 0)
        {
            _spaceInterval -= delta;
            return;
        }
        
        if (Input.IsKeyPressed(Key.Space))
        {
            var builtinLogAgent = GLog.GetAgent<BuiltinLogAgent>();
            if (builtinLogAgent.PanelOpened)
            {
                builtinLogAgent.CloseLogPanel();
            }
            else
            {
                builtinLogAgent.OpenLogPanel();
            }
            
            _spaceInterval = 1;
        }
    }
}