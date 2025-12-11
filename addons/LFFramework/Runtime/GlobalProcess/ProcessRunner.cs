using System;
using Godot;

namespace LF;

public partial class ProcessRunner:Node
{
    public event Action<double> OnProcess;
    public event Action<double> OnPhysicsProcess;

    public override void _Process(double delta)
    {
        base._Process(delta);
        OnProcess.SafeInvoke(delta);
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        OnPhysicsProcess.SafeInvoke(delta);
    }
}