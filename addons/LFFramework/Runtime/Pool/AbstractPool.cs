using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GDLog;

namespace LF;

public abstract class AbstractPool<T>:IDisposable where T: class
{
    protected readonly HashSet<T> Set;
    protected readonly Action<T> ActionOnGet;
    protected readonly Action<T> ActionOnRelease;
    protected readonly Action<T> ActionOnDestroy;
    protected T FreshlyReleased;
    protected readonly int MaxSize;
    protected readonly bool CollectionCheck;

    public virtual int CountAll { get; protected set; }

    public virtual int CountActive => CountAll - CountInactive;

    public virtual int CountInactive => Set.Count + (FreshlyReleased != null ? 1 : 0);

    public AbstractPool(
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
    {
        if (maxSize <= 0)
        {
            throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));
        }

        Set = new HashSet<T>(defaultCapacity);
        MaxSize = maxSize;
        ActionOnGet = actionOnGet;
        ActionOnRelease = actionOnRelease;
        ActionOnDestroy = actionOnDestroy;
        CollectionCheck = collectionCheck;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual bool TryGetPooled(out T item)
    {
        item = null;
        if (FreshlyReleased != null)
        {
            item = FreshlyReleased;
            FreshlyReleased = null;
        }
        else if (Set.Count > 0)
        {
            item = Set.First();
            Set.Remove(item);
        }
        return item != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Release(T element)
    {
        if (CollectionCheck && (Set.Count > 0 || FreshlyReleased != null))
        {
            if (element == FreshlyReleased)
            {
                GLog.Error("重复回收对象");
                return;
            }

            if (Set.Contains(element))
            {
                GLog.Error("重复回收对象");
                return;
            }
        }

        ActionOnRelease.SafeInvoke(element);
        if (FreshlyReleased == null)
        {
            FreshlyReleased = element;
        }
        else if (CountInactive < MaxSize)
        {
            Set.Add(element);
        }
        else
        {
            CountAll--;
            ActionOnDestroy.SafeInvoke(element);
        }
    }

    public virtual void Clear()
    {
        if (ActionOnDestroy != null)
        {
            foreach (var obj in Set)
            {
                ActionOnDestroy(obj);
            }

            if (FreshlyReleased != null)
            {
                ActionOnDestroy(FreshlyReleased);
            }
        }

        FreshlyReleased = null;
        Set.Clear();
        CountAll = 0;
    }

    public virtual void Dispose() => Clear();
}