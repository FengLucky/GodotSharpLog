using System;
using Config;
using Cysharp.Threading.Tasks;
using GDLog;
using Godot;

namespace LF;

public class LFInitializationParam
{
    public string ThemePath { get; init; }
    public float MinWindowAspect { get; init; } = 4f/3;
    public float MaxWindowAspect { get; init; } = 21f/9;
    public bool LockDefaultWindowAspect { get; init; } = true;
}

public static partial class LFFramework
{
    public static async UniTask<bool> Initialization(LFInitializationParam param = null)
    {
        var hasError = false;
        param ??= new LFInitializationParam();
        hasError |= InitLog();
        hasError |= InitSettings();
        hasError |= await InitPackage();
        hasError |= InitTables();
        hasError |= await InitPageManager(param);
        hasError |= InitLocalization();
        hasError |= InitWindowAspectAdapter(param);

        return hasError;
    }

    private static bool InitSettings()
    {
        try
        {
            Settings.Init();
        }
        catch (Exception e)
        {
            GLog.Error($"Settings 初始化失败:{e.Message}");
            return false;
        }

        return true;
    }

    private static bool InitLog()
    {
        try
        {
            var fileLogAgent = new FileLogAgent(); 
            fileLogAgent.Cleanup();
            GLog.AddAgent(fileLogAgent);

            if (EngineDebugger.IsActive())
            {
                var debuggerLogAgent = new DebuggerLogAgent(); 
                GLog.AddAgent(debuggerLogAgent);
            }
            var godotLogAgent = new GodotLogAgent();
            GLog.AddAgent(godotLogAgent);

            var builtinLogAgent = new BuiltinLogAgent(); 
            GLog.AddAgent(builtinLogAgent);
        }
        catch (Exception e)
        {
            GD.PrintErr($"GLog 初始化失败:{e.Message}");
        }

        return false;
    }
    
    private static UniTask<bool> InitPackage()
    {
        return UniTask.FromResult(true);
    }

    private static bool InitTables()
    {
        try
        {
            Tables.LoadTables();
            return true;
        }
        catch (Exception e)
        {
            GLog.Error($"表格数据初始化失败:{e.Message}");
        }

        return false;
    }

    private static async UniTask<bool> InitPageManager(LFInitializationParam param)
    {
        try
        {
            PageManager.Instantiate();
        }
        catch (Exception e)
        {
            GLog.Error($"界面管理器初始化失败:{e.Message}");
            return false;
        }

        if (param.ThemePath.IsNotNullOrWhiteSpace())
        {
            try
            {
                await PageManager.Instance.SetTheme(param.ThemePath);
            }
            catch (Exception e)
            {
                GLog.Error($"设置界面主题失败:{e.Message}");
                return false;
            }
        }

        return true;
    }

    private static bool InitLocalization()
    {
        try
        {
            Localization.Init();
            return true;
        }
        catch (Exception e)
        {
            GLog.Error($"本地化初始化失败:{e.Message}");
        }
        return false;
    }

    private static bool InitWindowAspectAdapter(LFInitializationParam param)
    {
        try
        {
            var adapter = new WindowAspectAdapter(param.MinWindowAspect, param.MaxWindowAspect, param.LockDefaultWindowAspect);
            AddLastingNode(adapter);
            return true;
        }
        catch (Exception e)
        {
            GLog.Error($"窗口比例适配器初始化失败:{e.Message}");
        }
        return false;
    }
}