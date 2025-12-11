using System.Runtime.CompilerServices;
using Godot;

namespace LF;
public static class StringExpansion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrEmpty(this string s)
    {
        return !string.IsNullOrEmpty(s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrWhiteSpace(this string s)
    {
        return string.IsNullOrWhiteSpace(s);
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrWhiteSpace(this string s)
    {
        return !string.IsNullOrWhiteSpace(s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidResource(this string s)
    {
        if ((s.StartsWith("res://") || s.StartsWith("uid://")) && FileAccess.FileExists(s))
        {
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotValidResource(this string s)
    {
        return !s.IsValidResource();
    }
}