using System;
using System.Collections.Generic;
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
    private static LruMap<string, long> recentMessages = new(1000);
    private static long messageFrequencyMs = 1000;

    public static void SetLimitedMessageFrequencyMs(long ms) {
        messageFrequencyMs = ms;
    }

    public static void Error<T>(T msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.ERROR, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Warn<T>(T msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.WARN, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Info<T>(T msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.INFO, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Debug<T>(T msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.DEBUG, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Trace<T>(T msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.TRACE, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Log<T>(LogLevel level, T msg, string caller, string filePath, int lineNumber, bool limitDuplicates) {
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
        string msgString = $"{level,-5} [{name,50}] - {msg}";

        if (limitDuplicates) {
            var nowMS = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var last = recentMessages.Get(msgString);
            if (nowMS - last < messageFrequencyMs) {
                // Logged recently
                return;
            }

            recentMessages.Set(msgString, nowMS);
        }

        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        msgString = $"[{date}] {msgString}";

        switch (level) {
            case LogLevel.ERROR:
            case LogLevel.WARN:
                GD.PrintErr(msgString);
                break;

            default:
                GD.Print(msgString);
                break;
        }
    }
}