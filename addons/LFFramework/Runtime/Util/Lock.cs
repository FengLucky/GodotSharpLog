using System;
using System.Collections.Generic;
using IncrementLockHandle = System.UInt32;

namespace LF;
public class IncrementLock
{
    private IncrementLockHandle _internalHandle;
    private readonly HashSet<IncrementLockHandle> _lockSet;
    
    public IncrementLock()
    {
        _internalHandle = 0;
        _lockSet = new HashSet<uint>();
    }
    
    public IncrementLockHandle Lock()
    {
        if (_internalHandle < IncrementLockHandle.MaxValue)
        {
            _internalHandle++;
        }
        else
        {
            _internalHandle = 0;
        }
            
        _lockSet.Add(_internalHandle);
        return _internalHandle;
    }
    
    public void UnLock(IncrementLockHandle handle)
    {
        _lockSet.Remove(handle);
    }
    
    public bool IsLocked(IncrementLockHandle handle)
    {
        return _lockSet.Contains(handle);
    }
    
    public bool IsLocked()
    {
        return _lockSet.Count > 0;
    }
}
    
public class TypeLock<T> where T:Enum
{
    private readonly HashSet<T> _lockSet;
    
    public TypeLock()
    {
        _lockSet = new HashSet<T>();
    }
    
    public void Lock(T type)
    {
        _lockSet.Add(type);
    }
    
    public void UnLock(T type)
    {
        _lockSet.Remove(type);
    }
    
    public bool IsLocked(T type)
    {
        return _lockSet.Contains(type);
    }
    
    public bool IsLocked()
    {
        return _lockSet.Count > 0;
    }

    public void Clear()
    {
        _lockSet.Clear();
    }
}
