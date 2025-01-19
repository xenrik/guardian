using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class Assert {
    public static AssertedType<T> OfType<T>(object value, string msg = "", [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        if (value is T) {
            return new AssertedType<T>((T)value);
        } else {
            if (msg == "") {
                msg = $"Object does not implement {typeof(T)}";
            }

            Logger.Log(LogLevel.ERROR, msg, caller, filePath, lineNumber);
            return new AssertedType<T>(default(T));
        }
    }

    public static void Equals<T>(T left, T right, string msg = "", [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        if (EqualityComparer<T>.Default.Equals(left, right)) {
            return;
        }

        if (msg == "") {
            msg = $"Expected {left},  buf found {right}";
        }
        Logger.Log(LogLevel.ERROR, msg, caller, filePath, lineNumber);
    }

    public class AssertedType<T> {
        private T value;

        public AssertedType(T value) {
            this.value = value;
        }

        public void WithType(Action<T> consumer) {
            if (value == null) {
                return;
            }

            consumer(value);
        }
    }
}