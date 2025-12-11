using System;

namespace LF;
public class FSMStateBase<TState,TEnum> 
    where TState:FSMStateBase<TState,TEnum>  
    where TEnum:Enum
{
    public FSMControl<TState,TEnum> Control { get;}
    public TEnum Type { get;}

    public FSMStateBase(FSMControl<TState,TEnum> control, TEnum type)
    {
        Control = control;
        Type = type;
    }
    
    public virtual void OnEnter()
    {
        
    }
        
    public virtual bool CanSwitchTo(TEnum type)
    {
        return Control.CanSwitchTo(type);
    }

    public virtual bool CanSwitchTo(TState state)
    {
        return true;
    }

    public virtual bool CanSwitchFrom(TState state)
    {
        return true;
    }

    public virtual void OnUpdate(float deltaTime)
    {
        
    }

    public virtual void OnExit()
    {
        
    }
}