public static class ObjectUtils {
    public static int GetHashCode(params object[] values) {
        int hashCode = 1;
        foreach (object val in values) {
            hashCode = 31 * hashCode + (val != null ? val.GetHashCode() : 0);
        }

        return hashCode;
    }
}