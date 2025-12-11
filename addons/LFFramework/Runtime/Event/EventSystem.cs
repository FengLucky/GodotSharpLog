using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GDLog;

namespace LF
{
    public class EventSystem<T> where T:Enum
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<T, HashSet<Action<object>>> _eventActionDict = new();

        private bool _inSend;
        
        private Action _onAfterSend;
        
        public void SetAfterSendCallback(Action callback)
        {
            _onAfterSend = callback;
        }

        public void DeferRegister(T eventId, Action<object> onReceive)
        {
            DeferRegisterInternal(eventId, onReceive).Forget();
        }

        private async UniTaskVoid DeferRegisterInternal(T eventId, Action<object> onReceive)
        {
            await UniTask.Yield();
            Register(eventId, onReceive);
        }
        
        /// <summary>
        /// 注册事件监听
        /// </summary>
        /// <param name="eventId"> 事件 id </param>
        /// <param name="onReceive"> 触发事件时回调 </param>
        public void Register(T eventId, Action<object> onReceive)
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
                _eventActionDict.Add(eventId, new HashSet<Action<object>>());
            }

            var set = _eventActionDict[eventId];

            if (!set.Add(onReceive))
            {
                GLog.Error($"重复注册监听事件回调方法:{eventId}");
            }
        }
        
        public void DeferUnRegister(T eventId, Action<object> onReceive)
        {
            DeferUnRegisterInternal(eventId, onReceive).Forget();
        }
        
        private async UniTaskVoid DeferUnRegisterInternal(T eventId, Action<object> onReceive)
        {
             await UniTask.Yield();
            UnRegister(eventId, onReceive);
        }

        /// <summary>
        /// 注销指定事件指定监听回调
        /// </summary>
        /// <param name="eventId"> 事件 id </param>
        /// <param name="onReceive"> 注销的事件回调 </param>
        public void UnRegister(T eventId, Action<object> onReceive)
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
        
        public void DeferUnRegisterAll(T eventId)
        {
            DeferUnRegisterInternal(eventId).Forget();
        }
        
        private async UniTaskVoid DeferUnRegisterInternal(T eventId)
        {
            await UniTask.DelayFrame(1);
            UnRegisterAll(eventId);
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
        public void Send(T eventId, object args = null)
        {
            if (_eventActionDict.TryGetValue(eventId, out var set))
            {
                _inSend = true;

                foreach (var action in set)
                {
                    action.SafeInvoke(args);
                }

                _inSend = false;
                _onAfterSend.SafeInvoke();
            }
        }


        /// <summary>
        ///  延迟到当前帧结尾再发送事件
        /// </summary>
        /// <param name="eventId">事件 id</param>
        /// <param name="args">事件参数</param>
        /// <param name="onceEvent">true 相同事件只发送一次 false 相同事件相同参数只发送一次</param>
        public void DeferSend<TArgs>(T eventId, TArgs args = default,bool onceEvent = false) where TArgs : IEquatable<TArgs>
        {
            if (onceEvent)
            {
                
            }
            DeferSendInternal(eventId, args).Forget();
        }
        
        private readonly List<(T,object)> _deferEvents = new();
        private bool _waitDeferSend;
        private async UniTaskVoid DeferSendInternal(T eventId, object args)
        {
            var tuple = (eventID: eventId, args);
            var index = _deferEvents.IndexOf(tuple);
            if (index == -1)
            {
                _deferEvents.Add(tuple);
            }
            else
            {
                _deferEvents[index] = tuple;
            }

            if (_waitDeferSend)
            {
                return;
            }
            _waitDeferSend = true;
             await UniTask.Yield();
            _waitDeferSend = false;
            for (var i = 0; i < _deferEvents.Count; i++)
            {
                Send(_deferEvents[i].Item1, _deferEvents[i].Item2);
            }
            
            _deferEvents.Clear();
            _onAfterSend.SafeInvoke();
        }
        
        private readonly List<T> _deferOnceEvents = new();
        private readonly List<object> _deferOnceEventArgs = new();
        private async UniTaskVoid DeferOnceEventSendInternal(T eventId, object args)
        {
            var index = _deferOnceEvents.IndexOf(eventId);
            if (index == -1)
            {
                _deferOnceEvents.Add(eventId);
                _deferOnceEventArgs.Add(args);
            }
            else
            {
                _deferOnceEventArgs[index] = args;
            }

            if (_waitDeferSend)
            {
                return;
            }
            _waitDeferSend = true;
             await UniTask.Yield();
            _waitDeferSend = false;
            for (var i = 0; i < _deferOnceEvents.Count; i++)
            {
                Send(_deferOnceEvents[i], _deferOnceEventArgs[i]);
            }
            
            _deferOnceEvents.Clear();
            _deferOnceEventArgs.Clear();
            _onAfterSend.SafeInvoke();
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