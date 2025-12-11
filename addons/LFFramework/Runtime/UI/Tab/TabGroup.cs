// using System;
// using System.Collections.Generic;
//
// namespace LF;
//
// public class TabGroup<T, TData> where T : TabBase<TData>
// {
//     public T CurrentSelect { get; private set; }
//     public event Action<T> OnClickTab;
//     public event Action<T> OnRepeatedSelectTab;
//     public event Action<T> OnSelectTab;
//
//     private readonly UIList<T, TData> _tabList = new();
//
//     public void Init(T tabTemplate, RectTransform parent = null)
//     {
//         _tabList.Init(tabTemplate, parent);
//         _tabList.OnNewItem += OnNewTab;
//     }
//
//     public void Refresh(IList<TData> dataList, int defaultSelect = -1)
//     {
//         _tabList.Refresh(dataList);
//         if (defaultSelect > -1)
//         {
//             Select(defaultSelect);
//         }
//     }
//
//     public void Select(int index)
//     {
//         Select(_tabList.Get(index));
//     }
//
//     public void Select(T tab)
//     {
//         if (!tab.CanSelect())
//         {
//             return;
//         }
//
//         if (tab)
//         {
//             OnClickTab.SafeInvoke(tab);
//         }
//
//         if (CurrentSelect)
//         {
//             if (CurrentSelect == tab)
//             {
//                 OnRepeatedSelectTab.SafeInvoke(tab);
//                 return;
//             }
//
//             CurrentSelect.UnSelect();
//         }
//
//         CurrentSelect = tab;
//         if (CurrentSelect)
//         {
//             CurrentSelect.Select();
//             OnSelectTab.SafeInvoke(CurrentSelect);
//         }
//     }
//
//     private void OnNewTab(T tab)
//     {
//         tab.OnClickEvent += OnTabClick;
//     }
//
//     private void OnTabClick(TabBase<TData> tab)
//     {
//         Select(tab as T);
//     }
// }