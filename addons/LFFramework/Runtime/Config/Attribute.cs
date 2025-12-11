using System;
using System.Diagnostics;

namespace Luban;
[Conditional("UNITY_EDITOR")]
public class ConfigFieldAttribute:System.Attribute
{
        
}

[Conditional("UNITY_EDITOR")]
public class EnumAliasAttribute : Attribute
{
    public string Alias;

    public EnumAliasAttribute(string alias)
    {
        Alias = alias;
    }
}

[Conditional("UNITY_EDITOR")]
public class ConfigAttribute : Attribute
{
        
}

[Conditional("UNITY_EDITOR")]
public class OriginalFieldNameAttribute : Attribute
{
    public string Name;

    public OriginalFieldNameAttribute(string name)
    {
        Name = name;
    }
}

[Conditional("UNITY_EDITOR")]
public class ConfigFilePathAttribute : Attribute
{
    public string Path;

    public ConfigFilePathAttribute(string path)
    {
        Path = path;
    }
}