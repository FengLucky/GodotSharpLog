using System;
using System.Runtime.CompilerServices;
using GDLog;

namespace LF;

public static class ActionExpansion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke(this Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke<T>(this Action<T> action, T arg)
    {
        try
        {
            action?.Invoke(arg);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
    {
        try
        {
            action?.Invoke(arg1, arg2);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
    {
        try
        {
            action?.Invoke(arg1, arg2, arg3);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3,
        T4 arg4)
    {
        try
        {
            action?.Invoke(arg1, arg2, arg3, arg4);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2,
        T3 arg3, T4 arg4, T5 arg5)
    {
        try
        {
            action?.Invoke(arg1, arg2, arg3, arg4, arg5);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> action, T1 arg1,
        T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        try
        {
            action?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SafeInvoke<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> action,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        try
        {
            action?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }
    }
}