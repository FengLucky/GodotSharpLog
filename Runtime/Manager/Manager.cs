using System;
using GDLog;

namespace LF;

public abstract class Manager<T> where T : Manager<T>, new()
{
    private static T _instance;
    private static readonly object Lock = new();

    public static T Instance => Instantiate();

    public static T Instantiate()
    {
        if (_instance == null)
        {
            lock (Lock)
            {
                if (_instance == null)
                {
                    _instance = new T();
                    try
                    {
                        _instance.OnInit();
                    }
                    catch (Exception e)
                    {
                        GLog.Exception(e);
                    }
                }
            }
        }
        return _instance;
    }
    protected virtual void OnInit(){}
}