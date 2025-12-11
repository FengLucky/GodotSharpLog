using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GDLog;

namespace LF
{
    public class AsyncEventSystem<T> where T : Enum
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<T, HashSet<Func<object,UniTask>>> _eventActionDict = new();

        private bool _inSend;
        private Action _onAfterSend;

        public void SetAfterSendCallback(Action callback)
        {
            _onAfterSend = callback;
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        /// <param name="eventId"> 事件 id </param>
        /// <param name="onReceive"> 触发事件时回调 </param>
        public void Register(T eventId, Func<object,UniTask> onReceive)
        {
            if (!CanModifyDict())
            {
                return;
            }

            if (onReceive == null)
            {
                GLog.Error("事件回调不能为空");
                return;
            }

            if (!_eventActionDict.ContainsKey(eventId))
            {
                _eventActionDict.Add(eventId, new HashSet<Func<object,UniTask>>());
            }

            var set = _eventActionDict[eventId];

            if (!set.Add(onReceive))
            {
                GLog.Error($"重复注册监听事件回调方法:{eventId}");
            }
        }

        /// <summary>
        /// 注销指定事件指定监听回调
        /// </summary>
        /// <param name="eventId"> 事件 id </param>
        /// <param name="onReceive"> 注销的事件回调 </param>
        public void UnRegister(T eventId, Func<object,UniTask> onReceive)
        {
            if (!CanModifyDict())
            {
                return;
            }

            if (_eventActionDict.TryGetValue(eventId, out var set))
            {
                set.Remove(onReceive);
            }
        }

        /// <summary>
        /// 注销指定事件所有监听回调
        /// </summary>
        /// <param name="eventId"> 事件 id </param>
        public void UnRegisterAll(T eventId)
        {
            if (!CanModifyDict())
            {
                return;
            }

            if (_eventActionDict.TryGetValue(eventId, out var set))
            {
                set.Clear();
            }
        }

        public void UnRegisterAll()
        {
            if (!CanModifyDict())
            {
                return;
            }
            _eventActionDict.Clear();
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventId"> 事件 id </param>
        /// <param name="args"> 事件参数(可选) </param>
        public async UniTask Send(T eventId, object args = null)
        {
            if (_eventActionDict.TryGetValue(eventId, out var set))
            {
                _inSend = true;
                foreach (var action in set)
                {
                    try
                    {
                        await action.Invoke(args);
                    }
                    catch (Exception e)
                    {
                        GLog.Exception(e);
                    }
                }
                _inSend = false;
                _onAfterSend.SafeInvoke();
            }
        }

        private bool CanModifyDict()
        {
            if (_inSend)
            {
                GLog.Error("正在执行监听事件，不可新增或删除监听");
            }

            return true;
        }
    }
}