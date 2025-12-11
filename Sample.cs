using System;
using Godot;
using LF;

public partial class Sample : Node
{
    private double _spaceInterval = 0;
    public override async void _EnterTree()
    {
        try
        {
            await LFFramework.Initialization();
            GD.Print("框架初始化完成");
        }
        catch (Exception e)
        {
            GD.PushError(e);
        }
    }
}