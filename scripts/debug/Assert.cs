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

    public static void AreEqual<T>(T expected, T actual, string msg = "", [CallerMemberName] string caller = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
        if (EqualityComparer<T>.Default.Equals(expected, actual)) {
            return;
        }

        if (msg == "") {
            var expectedStr = expected.ToString();
            // expectedStr = expectedStr.Length < 50 ? expectedStr : (expectedStr.Substring(0, 50) + "...");

            var actualStr = actual.ToString();
            // actualStr = actualStr.Length < 50 ? actualStr : (actualStr.Substring(0, 50) + "...");

            msg = $"Expected {expectedStr},  but found {actualStr}";
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