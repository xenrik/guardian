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

    public static void Error(string msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.ERROR, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Warn(string msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.WARN, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Info(string msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.INFO, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Debug(string msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.DEBUG, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Trace(string msg, bool limitDuplicates = true, [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        Log(LogLevel.TRACE, msg, caller, filePath, lineNumber, limitDuplicates);
    }

    public static void Log(LogLevel level, string msg, string caller, string filePath, int lineNumber, bool limitDuplicates) {
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
        msg = $"{level,-5} [{name,50}] - {msg}";

        if (limitDuplicates) {
            var nowMS = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var last = recentMessages.Get(msg);
            if (nowMS - last < messageFrequencyMs) {
                // Logged recently
                return;
            }

            recentMessages.Add(msg, nowMS);
        }

        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        msg = $"[{date}] {msg}";

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