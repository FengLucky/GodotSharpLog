using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LF;

public static class ReflectionUtil
{
    public static List<Type> GetAllChildType(this Type baseType, bool containAbstract = false)
    {
        if (baseType == null)
        {
            return new List<Type>();
        }

        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type != baseType && (!containAbstract || type.IsAbstract) &&
                           (type.IsSubclassOf(baseType) || type.IsSubGenericOf(baseType))).ToList();
    }

    public static Type GetBaseGenericType(this Type type, int index = 0)
    {
        var t = type;
        while (t.BaseType != null && t.BaseType != typeof(object))
        {
            t = t.BaseType;
        }

        if (t.IsGenericType)
        {
            return t.GenericTypeArguments.TryGet(index);
        }

        return null;
    }

    public static bool IsSubGenericOf(this Type type, Type baseType)
    {
        if (type == null)
        {
            return false;
        }

        // 检查类型是否实现了泛型基类的一个具体实例
        return type.GetInterfaces()
            .Concat(new[] { type.BaseType })
            .Any(interfaceOrBaseType =>
                interfaceOrBaseType?.IsGenericType == true &&
                interfaceOrBaseType.GetGenericTypeDefinition() == baseType);
    }

    public static void InvokeMethod(this object obj, string methodName, params object[] args)
    {
        var method = obj.GetType()
            .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            return;
        }

        method.Invoke(obj, args);
    }

    public static T InvokeMethod<T>(this object obj, string methodName, params object[] args)
    {
        var method = obj.GetType()
            .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null)
        {
            return default;
        }

        return (T)method.Invoke(obj, args);
    }

    private static readonly Dictionary<string, Type> CacheTypes = new();

    public static Type GetTypeByName(string typeName)
    {
        if (CacheTypes.TryGetValue(typeName, out var type))
        {
            return type;
        }

        type = Type.GetType(typeName);
        CacheTypes.Add(typeName, type);
        return type;
    }
}