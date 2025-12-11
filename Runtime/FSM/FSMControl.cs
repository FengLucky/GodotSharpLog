using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GDLog;

namespace LF;
 public class FSMControl<TState,TEnum> 
        where TState : FSMStateBase<TState,TEnum>
        where TEnum : Enum
    {
        private readonly Dictionary<TEnum, TState> _stateMap = new();
        public TState CurState { get;private set; }
        public TState LastState { get; private set; }

        public virtual void RegisterState(TState state,bool isDefault = false)
        {
            if (state == null)
            {
                return;
            }

            if (state.Control != this)
            {
                GLog.Error($"状态控制器不是当前控制器，无法注册:{state.Type}");
                return;
            }
            
            if (!_stateMap.TryAdd(state.Type, state))
            {
                GLog.Error($"状态机重复注册状态:{state.Type}");
                return;
            }

            if (isDefault)
            {
                if (CurState != null)
                {
                    GLog.Error($"状态机不能有多个默认状态:{state.Type}");
                    return;
                }

                CurState = state;
                state.OnEnter();
            }
        }

        public virtual bool SwitchState(TEnum type)
        {
            if (!_stateMap.TryGetValue(type,out var state))
            {
                GLog.Error($"{type} 状态未注册");
                return false;
            }

            if (LastState != null && !LastState.CanSwitchTo(state))
            {
                return false;
            }
            
            if (CurState != null && !CurState.CanSwitchFrom(state))
            {
                return false;
            }
            
            SwitchStateAsync(state).Forget();
            return true;
        }

        public bool CanSwitchTo(TEnum type)
        {
            if (!_stateMap.TryGetValue(type,out var state))
            {
                return false;
            }

            if (CurState != null && !CurState.CanSwitchTo(state))
            {
                return false;
            }
            
            if (!state.CanSwitchFrom(CurState))
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 切换状态在下一帧执行，防止逻辑错误
        /// </summary>
        protected virtual async UniTaskVoid SwitchStateAsync(TState state)
        {
            await UniTask.NextFrame();
            CurState?.OnExit();
            LastState = CurState;
            CurState = state;
            CurState.OnEnter();
        }

        public virtual void Update(float deltaTime)
        {
            CurState?.OnUpdate(deltaTime);
        }
    }