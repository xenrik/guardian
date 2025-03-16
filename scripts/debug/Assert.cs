using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class Assert {
    public static AssertedType<T> IsType<T>(object value, string msg = "", [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        if (value is T) {
            return new AssertedType<T>((T)value);
        } else {
            if (msg == "") {
                msg = $"Object does not implement {typeof(T)}";
            }

            Logger.Log(LogLevel.ERROR, msg, caller, filePath, lineNumber, false);
            return new AssertedType<T>(default(T));
        }
    }

    public static bool NotNull(object value, string msg = "", [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        if (value != null) {
            return true;
        } else {
            if (msg == "") {
                msg = $"Unexpected null object";
            }

            Logger.Log(LogLevel.ERROR, msg, caller, filePath, lineNumber, false);
            return false;
        }
    }

    public static bool AreEqual<T>(T expected, T actual, string msg = "", [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        if (EqualityComparer<T>.Default.Equals(expected, actual)) {
            return true;
        }

        if (msg == "") {
            var expectedStr = expected.ToString();
            // expectedStr = expectedStr.Length < 50 ? expectedStr : (expectedStr.Substring(0, 50) + "...");

            var actualStr = actual.ToString();
            // actualStr = actualStr.Length < 50 ? actualStr : (actualStr.Substring(0, 50) + "...");

            msg = $"Expected {expectedStr},  but found {actualStr}";
        }
        Logger.Log(LogLevel.ERROR, msg, caller, filePath, lineNumber, false);
        return false;
    }

    public class AssertedType<T> {
        private T value;

        public AssertedType(T value) {
            this.value = value;
        }

        public AssertedType<T> WhenTrue(Action<T> consumer) {
            if (value != null) {
                consumer(value);
            }

            return this;
        }

        public AssertedType<T> WhenFalse(Action<T> consumer) {
            if (value == null) {
                consumer(value);
            }

            return this;
        }
    }
}