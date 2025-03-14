using System;
using System.Collections;
using System.Collections.Generic;
using Godot;

public static class CollectionExtensions {
    public static bool IsEmpty(this ICollection collection) {
        return collection.Count == 0;
    }
    public static bool IsEmpty(this IEnumerable collection) {
        return !collection.GetEnumerator().MoveNext();
    }

    public static bool NotEmpty(this ICollection collection) {
        return collection.Count != 0;
    }
    public static bool NotEmpty(this IEnumerable collection) {
        return collection.GetEnumerator().MoveNext();
    }

    public static void ForEach<T>(this ICollection<T> collection, Action<T> action) {
        foreach (T element in collection) {
            action(element);
        }
    }

    public static Godot.Collections.Array<T> ToGodotArray<[MustBeVariant] T>(this ICollection<T> collection) {
        return new(collection);
    }
}