using GDLog;
using Godot;

namespace LF;

public static class Settings
{
    private const string SettingPath = "user://Setting/setting.cfg";
    private const string Section = "settings";
    private const int LargeStringLength = 1024;
    private static readonly ConfigFile Config = new();
    private static bool _initialized;
    
    public static void Init()
    {
        if (_initialized)
        {
            return;
        }
        
        _initialized = true;
        if (FileAccess.FileExists(SettingPath))
        {
            Config.Load(SettingPath);
        }
    }

    public static bool HasValue(string key)
    {
        return Config.HasSectionKey("settings", key);
    }

    public static void SetValue<[MustBeVariant] T>(string key, T value)
    {
        Config.SetValue(Section,key,Variant.From(value));
        Config.Save(SettingPath);
    }

    public static T GetValue<[MustBeVariant] T>(string key, T defaultValue = default)
    {
        var value = Config.GetValue(Section, key, Variant.From(defaultValue));
#if DEBUG
        if (value.Obj?.GetType() != typeof(T))
        {
            GLog.Warn($"Settings key '{key}' 类型是 {value.Obj?.GetType()} 不是要获取的 {typeof(T)}");      
        }
#endif
        return value.As<T>();
    }
}