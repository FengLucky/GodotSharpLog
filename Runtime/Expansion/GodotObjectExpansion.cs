using System.Runtime.CompilerServices;
using Godot;

namespace LF;

public static class GodotObjectExpansion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(this GodotObject obj)
    {
        return obj!= null && GodotObject.IsInstanceValid(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotValid(this GodotObject obj)
    {
        return !GodotObject.IsInstanceValid(obj);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static WeakRef WeakRef(this GodotObject obj)
    {
        return GodotObject.WeakRef(obj);
    }
}