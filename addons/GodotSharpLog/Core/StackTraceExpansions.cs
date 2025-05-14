using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace GDLog;

public static class StackTraceExpansions
{
    [ThreadStatic]
    private static StringBuilder _sb;
    private static readonly string ApplicationPath = ProjectSettings.GlobalizePath("res://");
    
    public static string Format(this StackTrace stackTrace,bool indent = true,bool beautiful = false)
    {
        if (_sb == null)
        {
            _sb = new();
        }
        else
        {
            _sb.Clear();
        }
        
        var firstLine = true;
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            var frame = stackTrace.GetFrame(i);
            if (frame.GetILOffset() != -1)
            {
                var fileName = frame.GetFileName();
                if (fileName != null && fileName.StartsWith("/root/godot/modules/mono"))
                {
                    break;
                }
            }

            var method = frame?.GetMethod();
            if (method != null && (ShowInStackTrace(method) || i == stackTrace.FrameCount))
            {
                if (firstLine)
                {
                    firstLine = false;
                }
                else
                {
                    _sb.AppendLine();
                }

                if (indent)
                {
                    _sb.Append("   ");
                }
                
                var declaringType = method.DeclaringType;
                var name1 = method.Name;
                var isStateMachineMethod = false;
                if (declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute), false))
                {
                    if (declaringType.IsAssignableTo(typeof(IAsyncStateMachine)) || declaringType.IsAssignableTo(typeof(IEnumerator)))
                    {
                        isStateMachineMethod = TryResolveStateMachineMethod(ref method, out declaringType);
                    }
                }

                if (declaringType != null)
                {
                    foreach (char ch in declaringType.FullName)
                    {
                        _sb.Append(ch == '+' ? '.' : ch);
                    }
                    _sb.Append('.');
                }

                _sb.Append(method.Name);
                var methodInfo = method as MethodInfo;
                if ((object)methodInfo != null && methodInfo.IsGenericMethod)
                {
                    var genericArguments = methodInfo.GetGenericArguments();
                    _sb.Append('[');
                    var index2 = 0;
                    var isFirstArgument = true;
                    for (; index2 < genericArguments.Length; ++index2)
                    {
                        if (!isFirstArgument)
                        {
                            _sb.Append(','); 
                        }
                        else
                        {
                            isFirstArgument = false;
                        }
                        _sb.Append(genericArguments[index2].Name);
                    }

                    _sb.Append(']');
                }

                ParameterInfo[] parameterInfoArray = null;
                try
                {
                    parameterInfoArray = method.GetParameters();
                }
                catch
                {
                    // ignored
                }

                if (parameterInfoArray != null)
                {
                    _sb.Append('(');
                    var isFirstParameter = true;
                    foreach (var info in parameterInfoArray)
                    {
                        if (!isFirstParameter)
                        {
                            _sb.Append(", ");
                        }
                        else
                        {
                            isFirstParameter = false;
                        }
                            
                        var str2 = "<UnknownType>";
                        if (info.ParameterType != null)
                            str2 = info.ParameterType.Name;
                        _sb.Append(str2);
                        var name2 = info.Name;
                        if (name2 != null)
                        {
                            _sb.Append(' ');
                            _sb.Append(name2);
                        }
                    }

                    _sb.Append(')');
                }

                if (isStateMachineMethod)
                {
                    _sb.Append('+');
                    _sb.Append(name1);
                    _sb.Append('(').Append(')');
                }

                if (frame.GetILOffset() != -1)
                {
                    var fileName = frame.GetFileName();
                    if (fileName != null)
                    {
                        _sb.Append(' ');
                        var relativePath = fileName.Replace(Path.DirectorySeparatorChar, '/').Replace(ApplicationPath,"");
                        var withResPath = "res://" + relativePath;
                        if (beautiful)
                        {
                            _sb.AppendFormat(
                                "(at [color=yellow][url={{\"path\":\"{0}\",\"line\":{2}}}]{1}:{2}[/url][/color])",
                                relativePath, withResPath, frame.GetFileLineNumber());
                        }
                        else
                        {
                            _sb.Append($"(at {withResPath}:{frame.GetFileLineNumber()})");
                        }
                    }
                }
            }
        }
        return _sb.ToString();
    }
    
    private static bool ShowInStackTrace(MethodBase mb)
    {
        if ((mb.MethodImplementationFlags & MethodImplAttributes.AggressiveInlining) != MethodImplAttributes.IL)
        {
            return false;
        }
           
        try
        {
            if (mb.IsDefined(typeof(StackTraceHiddenAttribute), false))
            {
                return false;
            }
               
            var declaringType = mb.DeclaringType;
            if (declaringType != null)
            {
                if (declaringType.IsDefined(typeof(StackTraceHiddenAttribute), false))
                {
                    return false;
                }
            }
        }
        catch
        {
            // ignored
        }

        return true;
    }
    
    private static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
    {
        declaringType = method.DeclaringType;
        var declaringType1 = declaringType?.DeclaringType;
        if (declaringType1 == null)
        {
            return false;
        }
           
        var declaredMethods = GetDeclaredMethods(declaringType1);
        if (declaredMethods == null)
        {
            return false;
        }
           
        foreach (var element in declaredMethods)
        {
            var customAttributes = (StateMachineAttribute[]) Attribute.GetCustomAttributes(element, typeof (StateMachineAttribute), false);
            var flag1 = false;
            var flag2 = false;
            foreach (var machineAttribute in customAttributes)
            {
                if (machineAttribute.StateMachineType == declaringType)
                {
                    flag1 = true;
                    flag2 = ((flag2 ? 1 : 0) | (machineAttribute is IteratorStateMachineAttribute ? 1 : (machineAttribute is AsyncIteratorStateMachineAttribute ? 1 : 0))) != 0;
                }
            }
            if (flag1)
            {
                method = element;
                declaringType = element.DeclaringType;
                return flag2;
            }
        }
        return false;

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2070:UnrecognizedReflectionPattern", Justification = "Using Reflection to find the state machine's corresponding method is safe because the corresponding method is the only caller of the state machine. If the state machine is present, the corresponding method will be, too.")]
        static MethodInfo[] GetDeclaredMethods(Type type)
        {
            return type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}