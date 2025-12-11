using System.Runtime.CompilerServices;
using Godot;

namespace LF;

public static class VectorExpansion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 NoY(this Vector3 vec)
    {
        return new Vector3(vec.X, 0, vec.Z);
    }
}