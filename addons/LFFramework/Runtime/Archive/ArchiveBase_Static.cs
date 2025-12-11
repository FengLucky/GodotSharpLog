using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using GDLog;
using Godot;
using FileAccess = Godot.FileAccess;

namespace LF;

public partial class ArchiveBase<T,ST>
{
    public static T Instance { get; private set; }
    public const string ArchiveRootPath = "user://Archive";
    private const string ArchiveListFile = "user://Archive/list.cfg";
    private const string QuickArchiveFile = "user://Archive/quick.archive";
    private const string QuickArchiveSimpleFile = "user://Archive/quick.simple";
    private const string ArchivePathKey = "archive_path";
    private const string ArchiveSimplePathKey = "archive_simple_path";
    private static readonly ConfigFile ArchiveList = new();
    private static bool _initialized;
    
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = true,
        IncludeFields = false,
        IgnoreReadOnlyProperties = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters =
        {
            new GodotTypeJsonConvert<Vector2>(),
            new GodotTypeJsonConvert<Vector2I>(),
            new GodotTypeJsonConvert<Rect2>(),
            new GodotTypeJsonConvert<Rect2I>(),
            new GodotTypeJsonConvert<Vector3>(),
            new GodotTypeJsonConvert<Transform2D>(),
            new GodotTypeJsonConvert<Vector4>(),
            new GodotTypeJsonConvert<Vector4I>(),
            new GodotTypeJsonConvert<Plane>(),
            new GodotTypeJsonConvert<Quaternion>(),
            new GodotTypeJsonConvert<Aabb>(),
            new GodotTypeJsonConvert<Transform3D>(),
            new GodotTypeJsonConvert<Projection>(),
            new GodotTypeJsonConvert<Color>(),
            new GodotTypeJsonConvert<NodePath>(),
        }
    };
    
    public static bool HasArchive(int index)
    {
        LoadList();
        return ArchiveList.HasSection(index.ToString());
    }

    public static bool HasQuickArchive()
    {
        return FileAccess.FileExists(QuickArchiveFile) && FileAccess.FileExists(QuickArchiveSimpleFile);
    }

    /// <summary>
    /// 获取存档数量，不包括快速存档
    /// </summary>
    /// <returns></returns>
    public static int GetArchiveCount()
    {
        LoadList();
        return ArchiveList.GetSections().Length;
    }

    /// <summary>
    /// 创建一个新存档
    /// </summary>
    /// <returns></returns>
    public static T CreateArchive()
    {
        Instance = new T();
        return Instance;
    }

    /// <summary>
    /// 删除指定存档
    /// </summary>
    /// <param name="index">存档位</param>
    public static bool RemoveArchive(int index)
    {
        LoadList();
        if (!ArchiveList.HasSection(index.ToString()))
        {
            return true;
        }

        try
        {
            var path = ArchiveList.GetValue(index.ToString(), ArchivePathKey).AsString();
            var simplePath = ArchiveList.GetValue(index.ToString(), ArchiveSimplePathKey).AsString();
            ArchiveList.EraseSection(index.ToString());
            ArchiveList.Save(ArchiveListFile);
            File.Delete(ProjectSettings.GlobalizePath(path));
            File.Delete(ProjectSettings.GlobalizePath(simplePath));
            return true;
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            return false;
        }
    }

    /// <summary>
    /// 删除快速存档
    /// </summary>
    public static bool RemoveQuickArchive()
    {
        try
        {
            var path = ProjectSettings.GlobalizePath(QuickArchiveFile);
            var simplePath = ProjectSettings.GlobalizePath(QuickArchiveSimpleFile);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            if (File.Exists(simplePath))
            {
                File.Delete(simplePath);
            }

            return true;
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            return false;
        }
    }
    
    /// <summary>
    /// 从指定存档位加载或创建一个存档
    /// </summary>
    public static bool LoadArchive(int index)
    {
        LoadList();

        if (!ArchiveList.HasSection(index.ToString()))
        {
            GLog.Error($"存档位 {index} 不存在");
            return false;
        }
        
        var path = ArchiveList.GetValue(index.ToString(), ArchivePathKey).AsString();
        return LoadArchive(path);
    }

    /// <summary>
    /// 加载快速存档
    /// </summary>
    public static bool LoadQuickArchive()
    {
        if (!HasQuickArchive())
        {
            return false;
        }
        return LoadArchive(QuickArchiveFile);
    }

    public static ST GetQuickSimpleArchive()
    {
        try
        {
            if (!HasQuickArchive())
            {
                return null;
            }
            return JsonSerializer.Deserialize<ST>(FileAccess.GetFileAsString(QuickArchiveSimpleFile));
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            return null;
        }
    }

    public static List<ST> GetSimpleArchives()
    {
        var list = new List<ST>();
        try
        {
            LoadList();
            foreach (var section in ArchiveList.GetSections())
            {
                var path = ArchiveList.GetValue(section, ArchiveSimplePathKey).AsString();
                list.Add(JsonSerializer.Deserialize<ST>(FileAccess.GetFileAsString(path)));
            }
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            list.Clear();
        }
        return list;
    }

    public static List<ST> GetSimpleArchivesWithQuick()
    {
        var simple = GetQuickSimpleArchive();
        var list = GetSimpleArchives();
        if (simple != null)
        {
            list.Insert(0,simple);
        }

        return list;
    }

    private static bool LoadArchive(string archivePath)
    {
        try
        {
            Instance = JsonSerializer.Deserialize<T>(FileAccess.GetFileAsString(archivePath));
            Instance.OnLoad();
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            Instance = null;
            return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LoadList()
    {
        if (_initialized)
        {
            return;
        }
        
        _initialized = true;
        if (FileAccess.FileExists(ArchiveListFile))
        {
            ArchiveList.Load(ArchiveListFile);
        }
    }
}