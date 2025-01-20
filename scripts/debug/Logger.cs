using System;
using System.IO;
using System.Runtime.CompilerServices;
using Godot;

public enum LogLevel {
    ERROR,
    WARN,
    INFO,
    DEBUG,
    TRACE
}

public static class Logger {
    public static void Error(string msg, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.ERROR, msg, caller, filePath, lineNumber);
    }

    public static void Warn(string msg, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.WARN, msg, caller, filePath, lineNumber);
    }

    public static void Info(string msg, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.INFO, msg, caller, filePath, lineNumber);
    }

    public static void Debug(string msg, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.DEBUG, msg, caller, filePath, lineNumber);
    }

    public static void Trace(string msg, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.TRACE, msg, caller, filePath, lineNumber);
    }

    public static void Log(LogLevel level, string msg, string caller, string filePath, int lineNumber) {
        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string name = caller + "@" + lineNumber;

        int offset = filePath.Length - (50 - name.Length - 5);
        if (offset < 0) {
            // Do nothing
        } else if (offset > filePath.Length) {
            filePath = "...";
        } else {
            filePath = "..." + Path.DirectorySeparatorChar + filePath.Substring(offset);
        }

        name = filePath + ";" + name;
        msg = $"[{date}] {level,-5} [{name,50}] - {msg}";

        switch (level) {
            case LogLevel.ERROR:
            case LogLevel.WARN:
                GD.PrintErr(msg);
                break;

            default:
                GD.Print(msg);
                break;
        }
    }
}