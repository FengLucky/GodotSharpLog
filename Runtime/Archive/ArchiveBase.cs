using System;
using System.IO;
using System.Text.Json;
using GDLog;
using Godot;

namespace LF;

public abstract partial class ArchiveBase<T,ST> where T:ArchiveBase<T,ST>,new() where ST:SimpleArchiveBase,new()
{
    /// <summary>
    /// 保存存档
    /// </summary>
    /// <param name="index">存档位</param>
    /// <returns></returns>
    public bool Save(int index)
    {
        if (index < 0)
        {
            GLog.Error("存档位必须大于 0");
            return false;
        }
        
        try
        {
            LoadList();
            string archivePath,simplePath;
            if (!ArchiveList.HasSection(index.ToString()))
            {
                var guid = Guid.NewGuid().ToString();
                archivePath = ArchiveRootPath + "/" + guid + ".archive";
                simplePath = ArchiveRootPath + "/" + guid + ".simple";
                ArchiveList.SetValue(index.ToString(),ArchivePathKey,archivePath);
                ArchiveList.SetValue(index.ToString(),ArchiveSimplePathKey,simplePath);
                ArchiveList.Save(ArchiveListFile);
            }
            else
            {
                archivePath = ArchiveList.GetValue(index.ToString(), ArchivePathKey).AsString();
                simplePath = ArchiveList.GetValue(index.ToString(), ArchiveSimplePathKey).AsString();
            }
            
            OnSave();
            return Save(index,archivePath, simplePath);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            return false;
        }
    }

    /// <summary>
    /// 保存快速存档，推荐用于自动保存
    /// </summary>
    public bool SaveQuick()
    {
        try
        {
            OnQuickSave();
            return Save(-1,QuickArchiveFile,QuickArchiveSimpleFile);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            return false;
        }
    }

    /// <summary>
    /// 简充简单存档数据
    /// </summary>
    /// <returns></returns>
    protected abstract void FillSimpleData(ST data);
    
    protected virtual void OnLoad()
    {
        
    }

    protected virtual void OnSave()
    {
        
    }

    protected virtual void OnQuickSave()
    {
        
    }

    private bool Save(int index,string archivePath,string simplePath)
    {
        try
        {
            var archive = JsonSerializer.Serialize(Instance, SerializerOptions);
            var simpleData = SimpleArchiveBase.CreateNew<ST>(index);
            FillSimpleData(simpleData);
            var simple = JsonSerializer.Serialize(simpleData,SerializerOptions);
            
            var directory = ProjectSettings.GlobalizePath(ArchiveRootPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(ProjectSettings.GlobalizePath(archivePath),archive);
            File.WriteAllText(ProjectSettings.GlobalizePath(simplePath),simple);
        }
        catch (Exception e)
        {
            GLog.Exception(e);
            return false;
        }

        return true;
    }
}