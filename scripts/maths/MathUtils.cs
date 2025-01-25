using System;
using Godot;

public static class MathUtils {
    public static T Clamp<T>(T value, T lower, T upper) where T : IComparable<T> {
        if (value.CompareTo(lower) <= 0) {
            return lower;
        } else if (value.CompareTo(upper) >= 0) {
            return upper;
        }

        return value;
    }
}