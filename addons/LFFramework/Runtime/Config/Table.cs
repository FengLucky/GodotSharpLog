using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;
using Luban;
using FileAccess = Godot.FileAccess;

namespace Config;

public partial class Tables
{
    private static bool _watched;
    private static bool _initialized;
    private static FileSystemWatcher _watcher;
    private static readonly List<string> ChangedList = new();

    private static readonly string BytePathRoot = "res://Data/Bin";
    private static readonly string JsonPathRoot = "res://Data/Json";
    
    public static void EditorInit()
    {
        if (!_initialized)
        {
            LoadTables(false);
        }
    }

    public static void LoadTables(bool watchChange = true)
    {
        var tablesCtor = typeof(Tables).GetMethod("LoadData", BindingFlags.NonPublic | BindingFlags.Static);
        if (tablesCtor != null)
        {
            var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];
            // 根据 Tables 的构造函数的Loader的返回值类型决定使用json还是ByteBuf Loader
            System.Delegate loader = loaderReturnType == typeof(ByteBuf)
                ? new System.Func<string, ByteBuf>(LoadByteBuf)
                : new System.Func<string, JsonElement>(LoadJson);
            tablesCtor.Invoke(null, [loader]);
#if TOOLS
            _initialized = true;
            if (watchChange)
            {
                if (!_watched)
                {
                    if (loaderReturnType == typeof(ByteBuf))
                    {
                        _watcher = new(ProjectSettings.GlobalizePath(BytePathRoot));
                        _watcher.Filter = "*.bytes";
                        _watcher.Changed += ByteChanged;
                    }
                    else
                    {
                        _watcher = new(ProjectSettings.GlobalizePath(JsonPathRoot));
                        _watcher.Filter = "*.json";
                        _watcher.Changed += JsonChanged;
                    }

                    _watcher.IncludeSubdirectories = true;
                    _watcher.EnableRaisingEvents = true;
                    _watched = true;
                }
            }
#endif
        }
    }

    private static JsonElement LoadJson(string file)
    {
        var path = $"{JsonPathRoot}/{file}.json";
        var json = FileAccess.GetFileAsString(path);
        return JsonDocument.Parse(json).RootElement;
    }

    private static ByteBuf LoadByteBuf(string file)
    {
        var path = $"{BytePathRoot}/{file}.bytes";
        var bytes = FileAccess.GetFileAsBytes(path);
        return new ByteBuf(bytes);
    }

    private static void JsonChanged(object sender, FileSystemEventArgs e)
    {
        if (e.Name != null)
        {
            var name = e.Name.Replace(".json", "");
            ApplyChange(name).Forget();
        }
    }

    private static void ByteChanged(object sender, FileSystemEventArgs e)
    {
        if (e.Name != null)
        {
            var name = e.Name.Replace(".bytes", "");
            ApplyChange(name).Forget();
        }
    }

    private static async UniTaskVoid ApplyChange(string name)
    {
#if TOOLS
        await UniTask.SwitchToMainThread();
        ChangedList.Add(name);
        int count = ChangedList.Count;
        await UniTask.WaitForSeconds(0.1f);
        if (count == ChangedList.Count)
        {
            var distinct = ChangedList.Distinct().ToList();
            var sb = new StringBuilder();
            foreach (var file in distinct)
            {
                sb.AppendLine($"{file}、");
            }

            sb.Remove(sb.Length - 1, 1);
            GLog.DebugInfo($"配置文件更新:{'{'}{sb}{'}'}");
            IncrementalUpdate(distinct);
            ChangedList.Clear();
        }
#endif
    }
}