using System;
using System.Runtime.CompilerServices;
using GDLog;

namespace LF;

public static class FuncExpansion
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T>(this Func<T> func)
    {
        try
        {
            if (func != null)
            {
                return func();
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T1, T>(this Func<T1, T> func, T1 arg1)
    {
        try
        {
            if (func != null)
            {
                return func(arg1);
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T1, T2, T>(this Func<T1, T2, T> func, T1 arg1, T2 arg2)
    {
        try
        {
            if (func != null)
            {
                return func(arg1, arg2);
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T1, T2, T3, T>(this Func<T1, T2, T3, T> func, T1 arg1, T2 arg2, T3 arg3)
    {
        try
        {
            if (func != null)
            {
                return func(arg1, arg2, arg3);
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T1, T2, T3, T4, T>(this Func<T1, T2, T3, T4, T> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        try
        {
            if (func != null)
            {
                return func(arg1, arg2, arg3, arg4);
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T1, T2, T3, T4, T5, T>(this Func<T1, T2, T3, T4, T5, T> func, T1 arg1, T2 arg2, T3 arg3,
        T4 arg4, T5 arg5)
    {
        try
        {
            if (func != null)
            {
                return func(arg1, arg2, arg3, arg4, arg5);
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T1, T2, T3, T4, T5, T6, T>(this Func<T1, T2, T3, T4, T5, T6, T> func, T1 arg1, T2 arg2,
        T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        try
        {
            if (func != null)
            {
                return func(arg1, arg2, arg3, arg4, arg5, arg6);
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SafeInvoke<T1, T2, T3, T4, T5, T6, T7, T>(this Func<T1, T2, T3, T4, T5, T6, T7, T> func, T1 arg1,
        T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        try
        {
            if (func != null)
            {
                return func(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
        }

        return default;
    }
}