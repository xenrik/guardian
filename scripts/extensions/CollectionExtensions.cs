using System.Collections;

public static class CollectionExtensions {
    public static bool IsEmpty(this ICollection collection) {
        return collection.Count == 0;
    }
    public static bool NotEmpty(this ICollection collection) {
        return collection.Count != 0;
    }
}