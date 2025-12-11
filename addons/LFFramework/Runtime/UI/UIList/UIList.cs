using System;
using System.Collections.Generic;
using GDLog;
using Godot;

namespace LF;
public class UIList<TItem, TData> where TItem : Control, IUIListItem<TData>
{
    private Control _template;
    private Control _parent;
    public List<TItem> ActiveItems { get; } = new();
    public List<TItem> AllItems { get; } = new();
    public event Action<TItem> OnNewItem;

    public void Init(TItem template,Control parent = null)
    {
        _template = template;
        _parent = parent;
        if (template.GetTree() != null)
        {
            template.Visible = false;
            if (parent == null)
            {
                _parent = template.GetParent() as Control;
            }
        }
        
        #if DEBUG
        if (_parent == null)
        {
            GLog.Error($"UIList 没有设置 item 的 parent");
        }
        #endif
    }
    
    public void Refresh(IList<TData> dataList)
    {
        if (dataList.IsNullOrEmpty())
        {
            Clear();
            return;
        }
        for (int i = AllItems.Count; i < dataList.Count; i++)
        {
            try
            {
                var item = _template.Duplicate() as TItem;
#if DEBUG
                if (item == null)
                {
                    GLog.Error($"UIList 模板元素不是 {typeof(TItem)}");
                    return;
                }
#endif
                _parent.AddChild(item);
                OnNewItem.SafeInvoke(item);
                AllItems.Add(item);
            }
            catch (Exception e)
            {
                GLog.Exception(e);
                return;
            }
        }
    
        ActiveItems.Clear();
        for (int i = 0; i < dataList.Count; i++)
        {
            AllItems[i].Visible = true;
            AllItems[i].Data = dataList[i];
            AllItems[i].Refresh();
            ActiveItems.Add(AllItems[i]);
        }
    
        for (int i = dataList.Count; i < AllItems.Count; i++)
        {
            AllItems[i].Visible = false;
        }
    }
    
    public TItem Get(int index)
    {
        return ActiveItems.TryGet(index);
    }
        
    public void Clear()
    {
        foreach (var item in AllItems)
        {
            item.Visible = false;
        }
        ActiveItems.Clear();
    }
}