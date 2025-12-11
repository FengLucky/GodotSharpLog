using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using GDLog;

namespace LF;

public partial class PageHolder
{
    private static readonly Dictionary<int, HashSet<PageHolder>> Holders = new();

    public static PageHolder Allocate(int configId)
    {
        var bean = Tables.PageTable.GetOrDefault(configId);
        if (bean == null)
        {
            GLog.Error($"界面配置不存在:{configId}");
            return null;
        }

        var type = !string.IsNullOrWhiteSpace(bean.HolderType)
            ? ReflectionUtil.GetTypeByName(bean.HolderType)
            : typeof(PageHolder);
        if (type == null)
        {
            GLog.Error($"当前程序集中找不到界面 {bean} 中配置的 holder 类型:{bean.HolderType}");
            return null;
        }

        var holder = Allocate(bean, type);
        if (holder == null)
        {
            GLog.Error($"界面配置 {bean} 中配置的 Holder 创建失败:{bean.HolderType}");
            return null;
        }
        return holder;
    }

    public static T Allocate<T>(int id) where T : PageHolder
    {
        return Allocate(id) as T;
    }

    private static PageHolder Allocate(PageBean bean, Type type)
    {
        if (!Holders.TryGetValue(bean.Id, out var set))
        {
            set = new();
            Holders.Add(bean.Id, set);
        }

        var cache = set.FirstOrDefault();
        if (cache?.Bean.Multiple == false)
        {
            return cache;
        }

        if (Activator.CreateInstance(type) is PageHolder holder)
        {
            holder._isValid = true;
            holder.Bean = bean;
            set.Add(holder);
            return holder;
        }

        return null;
    }
}