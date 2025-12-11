using System;
using System.Collections.Generic;

namespace Luban;
public abstract class TableBase
{
    public virtual void PreExport()
    {
        throw new Exception(GetType().Name + "未实现导出前操作");
    }

    public virtual List<object> GetConfigList()
    {
        throw new Exception(GetType().Name + "未实现获取所有配置操作");
    }
}