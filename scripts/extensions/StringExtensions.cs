public static class StringExtensions {
    public static string StripNewLines(this string val) {
        return val.Replace("\r\n", "").Replace("\n", "");
    }
}