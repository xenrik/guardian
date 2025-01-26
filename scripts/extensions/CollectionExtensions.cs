using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class CollectionExtensions {
    public static bool IsEmpty(this ICollection collection) {
        return collection.Count == 0;
    }

    public static bool NotEmpty(this ICollection collection) {
        return collection.Count != 0;
    }

    public static void ForEach<T>(this ICollection<T> collection, Action<T> action) {
        foreach (T element in collection) {
            action(element);
        }
    }
}