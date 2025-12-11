namespace LF;

public static class GlobalProcessExpansion
{
    public static void EnableProcess(this IProcess process, int priority = 100)
    {
        GlobalProcessManager.Instance.AddProcess(process,priority);
    }
    
    public static void EnablePhysicsProcess(this IPhysicsProcess process, int priority = 100)
    {
        GlobalProcessManager.Instance.AddPhysicsProcess(process,priority);
    }
    
    public static void DisableProcess(this IProcess process)
    {
        GlobalProcessManager.Instance.RemoveProcess(process);
    }
    
    public static void DisablePhysicsProcess(this IPhysicsProcess process)
    {
        GlobalProcessManager.Instance.RemovePhysicsProcess(process);
    }
}