using System;

namespace GDLog;

public interface ILogAgent
{
    void AddCategory(string category);
    void Info(object message,string category);
    void Warn(object message,string category);
    void Error(object message,string category);
    void Exception(Exception exception,string category);
}
