using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using LavaEngine.TimeCore;

namespace LavaEngine.Log;

public class Logger : IDisposable
{
    private string pattern = "[${Time.hour}:${Time.minute}:${Time.second}:${Time.millisecond}:${Time.microsecond}] [${Level}] [${Thread}] [${Caller}] ${Message}";
    private LogLevel minLogLevel = LogLevel.Trace;

    private static readonly object _consoleLock = new object();

    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }

    public Logger()
    {
    }

    ~Logger()
    {
    }

    public LogLevel MinLogLevel => minLogLevel;
    public string Pattern => pattern;

    public void SetMinLogLevel(LogLevel level) => minLogLevel = level;
    public void SetPattern(string pattern) => this.pattern = pattern;

    public void LogMes(string message,
                        LogLevel level = LogLevel.Info,
                        [CallerMemberName] string callerName = "",
                        [CallerFilePath] string callerPath = "",
                        [CallerLineNumber] int callerLine = 0)
    {
        if ((int)level < (int)minLogLevel)
        {
            return;
        }

        var sb = new StringBuilder(256);
        sb.Append(this.pattern);

        string levelString = level.ToString().ToUpper();
        sb.Replace("${Level}", levelString);

        int threadId = Thread.CurrentThread.ManagedThreadId;
        sb.Replace("${Thread}", threadId.ToString());

        string fileName = Path.GetFileName(callerPath);
        string callerInfo = $"{fileName}:{callerLine} -> {callerName}";
        sb.Replace("${Caller}", callerInfo);

        sb.Replace("${Message}", message);

        var time = new Time();
        sb.Replace("${Time.year}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.YEAR).ToString());
        sb.Replace("${Time.month}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.MONTH).ToString());
        sb.Replace("${Time.day}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.DAY).ToString());
        sb.Replace("${Time.hour}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.HOUR).ToString());
        sb.Replace("${Time.minute}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.MINUTE).ToString());
        sb.Replace("${Time.second}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.SECOND).ToString());
        sb.Replace("${Time.millisecond}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.MILLISECOND).ToString());
        sb.Replace("${Time.microsecond}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.MICROSECOND).ToString());
        sb.Replace("${Time.nanosecond}", time.getCurrentTimeOfSpecifiedLevel(Time.TimeLevel.NANOSECOND).ToString());

        ConsoleColor targetColor = GetColorByLevel(level);

        lock (_consoleLock)
        {
            Console.ResetColor();
            Console.ForegroundColor = targetColor;
            Console.WriteLine(sb.ToString());
            Console.ResetColor();
        }
    }

    private ConsoleColor GetColorByLevel(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace: return ConsoleColor.White;
            case LogLevel.Debug: return ConsoleColor.Cyan;
            case LogLevel.Info: return ConsoleColor.Green;
            case LogLevel.Warning: return ConsoleColor.Yellow;
            case LogLevel.Error: return ConsoleColor.Red;
            case LogLevel.Critical: return ConsoleColor.DarkRed;
            default: return ConsoleColor.White;
        }
    }

    public void Trace(string message) => LogMes(message, LogLevel.Trace);
    public void Debug(string message) => LogMes(message, LogLevel.Debug);
    public void Info(string message) => LogMes(message, LogLevel.Info);
    public void Warning(string message) => LogMes(message, LogLevel.Warning);
    public void Error(string message) => LogMes(message, LogLevel.Error);
    public void Critical(string message) => LogMes(message, LogLevel.Critical);

    public void Dispose()
    {
    }
}