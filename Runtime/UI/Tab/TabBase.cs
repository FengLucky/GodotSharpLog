// using System;
// using UnityEngine;
//
// namespace LF;
//
// public class TabBase<T> : UIMonoBehaviour, IUIListItem<T>
// {
//     [SerializeField] protected UIButton button;
//     public event Action<TabBase<T>> OnClickEvent;
//     public T Data { get; private set; }
//
//     private void Awake()
//     {
//         button.OnActiveClick = OnClick;
//     }
//
//     public virtual bool CanSelect()
//     {
//         return true;
//     }
//
//     public void Refresh(T data)
//     {
//         Data = data;
//         OnRefresh(data);
//     }
//
//     public void Select()
//     {
//         OnSelect();
//     }
//
//     public void UnSelect()
//     {
//         OnUnSelect();
//     }
//
//     protected virtual void OnSelect()
//     {
//     }
//
//     protected virtual void OnUnSelect()
//     {
//     }
//
//     protected virtual void OnRefresh(T data)
//     {
//     }
//
//     protected virtual void OnClick()
//     {
//         OnClickEvent.SafeInvoke(this);
//     }
// }