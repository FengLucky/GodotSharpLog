using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Godot;
using Environment = System.Environment;
using FileAccess = System.IO.FileAccess;

namespace GDLog;

public class FileLogAgentParam
{
    public long MaxFileSize { get; init; } = 50 * 1024 * 1024; // 50 M
    public LogLevel StackTraceLevel { get; init; } = LogLevel.Error;
    public string FilePath { get; init; }
    public string InfoFormat { get; init; } = "[Info]{0}";
    public string WarnFormat { get; init; } = "[Warn]{0}";
    public string ErrorFormat { get; init; } = "[Error]{0}";
    public string ExceptionFormat { get; init; } = "[Exception]{0}";
    public string StackTraceFormat { get; init; } = "{0}";
    public string CategoryFormat { get; init; } = "[{0}]";
    public string FullMessageWithoutStack { get; init; } = "{0}{1}";
    public string FullMessageFormat { get; init; } = "{0}{1}\n{2}";
    public string LogFilePrefix { get; init; } = "";
    public string LogFileSuffix { get; init; } = "";
}

public class FileLogAgent:ILogAgent
{
    private readonly long _maxFileSize;
    private readonly string _filePath;
    private readonly string _infoFormat;
    private readonly string _warnFormat;
    private readonly string _errorFormat;
    private readonly string _exceptionFormat;
    private readonly string _stackTraceFormat;
    private readonly string _categoryFormat;
    private readonly string _fullMessageWithoutStack;
    private readonly string _fullMessageFormat;
    private readonly string _logFilePrefix;
    private readonly string _logFileSuffix;
    private readonly LogLevel _stackTraceLevel;
    
    private int _fileIndex;
    private StreamWriter _writer;
    private string _currentFilePath;

    public event Action OnCreateLogFile;
    
    public FileLogAgent(FileLogAgentParam param = null,bool autoCleanup = true)
    {
        param ??= new();
        _maxFileSize = param.MaxFileSize;
        _stackTraceLevel = param.StackTraceLevel;
        _filePath = param.FilePath;
        _infoFormat = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}" : param.InfoFormat;
        _warnFormat = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}" : param.WarnFormat;
        _errorFormat = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}" : param.ErrorFormat;
        _exceptionFormat = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}" : param.ExceptionFormat;
        _stackTraceFormat = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}" : param.StackTraceFormat;
        _categoryFormat = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}" : param.CategoryFormat;
        _fullMessageWithoutStack = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}{1}" : param.FullMessageWithoutStack;
        _fullMessageFormat = string.IsNullOrWhiteSpace(param.InfoFormat) ? "{0}{1}\n{2}" : param.FullMessageFormat;
        _logFilePrefix = string.IsNullOrWhiteSpace(param.InfoFormat) ? "" : param.LogFilePrefix;
        _logFileSuffix = string.IsNullOrWhiteSpace(param.InfoFormat) ? "" : param.LogFileSuffix;
        
        if (_maxFileSize < 1024)
        {
            _maxFileSize = 1024;
        }
        
        if (string.IsNullOrWhiteSpace(_filePath))
        {
            _filePath = $"user://log/{DateTime.Now:yyyy-MM-dd_hh-mm-ss}_{Environment.ProcessId}.log";
        }

        CreateNewLogFile();

        if (autoCleanup)
        {
            Cleanup();
        }
    }
    
    public void AddCategory(string category)
    {
        
    }

    public void Info(object message, string category)
    {
        LogInternal(category,message,GetStack(LogLevel.Info),LogLevel.Info);
    }

    public void Warn(object message, string category)
    {
        LogInternal(category,message,GetStack(LogLevel.Warn),LogLevel.Warn);
    }

    public void Error(object message, string category)
    {
        LogInternal(category,message,GetStack(LogLevel.Error),LogLevel.Error);
    }

    public void Exception(Exception exception, string category)
    {
        LogInternal(category,exception.Message,GetStack(LogLevel.Exception),LogLevel.Exception);
    }

    private void LogInternal(string category, object message, StackTrace stackTrace,LogLevel level)
    {
        if (_writer == null)
        {
            return;
        }
        
        var formatCategory = string.IsNullOrWhiteSpace(category) ? "" : string.Format(_categoryFormat, category);
        string formatMessage = null;
        switch (level)
        {
            case LogLevel.Info:
                formatMessage = string.Format(_infoFormat, message);
                break;
            case LogLevel.Warn:
                formatMessage = string.Format(_warnFormat, message);
                break;
            case LogLevel.Error:
                formatMessage = string.Format(_errorFormat, message);
                break;
            case LogLevel.Exception:
                formatMessage = string.Format(_exceptionFormat, message);
                break;
        }

        if (stackTrace != null)
        {
            var formatFrame = string.Format(_stackTraceFormat, stackTrace.Format());
            _writer.WriteLine(_fullMessageFormat, formatCategory, formatMessage, formatFrame);
        }
        else
        {
            _writer.WriteLine(_fullMessageWithoutStack, formatCategory, formatMessage, "");
        }
        _writer.Flush();
        if (_writer.BaseStream.Position >= _maxFileSize)
        {
            CreateNewLogFile();
        }
    }

    public string GetCurrentLogPath()
    {
        return ProjectSettings.GlobalizePath(_currentFilePath);
    }

    public void Cleanup(uint saveCount = 20)
    {
        saveCount = Math.Max(1, saveCount);
        var directory = Path.GetDirectoryName(ProjectSettings.GlobalizePath(_filePath));
        var extension = Path.GetExtension(_filePath);
        directory ??= "";
        var files = new DirectoryInfo(directory).GetFiles($"*{extension}").ToList();
        files.Sort((item1,item2)=> (int)(item1.CreationTime - item2.CreationTime).TotalMilliseconds);
        for (int i = 0; i < files.Count - saveCount; i++)
        {
            try
            {
                File.Delete(files[i].FullName);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private StackTrace GetStack(LogLevel level)
    {
        if (level < _stackTraceLevel)
        {
            return null;
        }
        return new StackTrace(3,true);
    }
    
    private void CreateNewLogFile()
    {
        if (_writer != null)
        {
            if (!string.IsNullOrWhiteSpace(_logFileSuffix))
            {
                _writer.WriteLine(_logFileSuffix);
            }
            _writer.Close();
            _writer = null;
        }

        var fileName = Path.GetFileNameWithoutExtension(_filePath);
        var extension = Path.GetExtension(_filePath);
        string directory = string.Empty;
        if (fileName != null)
        {
            var index = _filePath.IndexOf(fileName, StringComparison.Ordinal);
            if (index > -1)
            {
                directory = _filePath.Substring(0, index);
            }
        }
       
        for (int i = 0; i < 100; i++)
        {
            try
            {
                var path =_fileIndex > 0 ?  $"{directory}{fileName}_{_fileIndex}{extension}" : $"{directory}{fileName}{extension}";
                path = ProjectSettings.GlobalizePath(path);
                var fullDirectory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(fullDirectory) && !Directory.Exists(fullDirectory))
                {
                    Directory.CreateDirectory(fullDirectory);
                }
                _writer = new StreamWriter(new FileStream(path, FileMode.CreateNew,FileAccess.Write,FileShare.ReadWrite));
                _currentFilePath = path;
                _fileIndex++;
                break;
            }
            catch (Exception e)
            {
                GD.PushError(e);
                _writer = null;
                _fileIndex++;
            }
        }

        if (_writer == null)
        {
            GD.PushError("create log file failure.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(_logFilePrefix))
        {
            _writer.WriteLine(_logFilePrefix);
        }

        try
        {
            OnCreateLogFile?.Invoke();
        }
        catch (Exception e)
        {
            GD.PushError(e);
        }
    }
}