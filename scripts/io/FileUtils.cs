using Godot;

public static class FileUtils {
    public static string GetParentPath(string path) {
        int lastForward = path.LastIndexOf("/");
        int lastBack = path.LastIndexOf("\\");

        if (lastForward <= 0 && lastBack <= 0) {
            return "";
        } else if (lastForward > lastBack) {
            return path.Substring(0, lastForward);
        } else {
            return path.Substring(0, lastBack);
        }
    }

    public static FileAccess Open(string path, FileAccess.ModeFlags flags, bool makeFolders = true) {
        if (makeFolders) {
            var parentFolder = GetParentPath(path);
            if (DirAccess.Open(parentFolder) == null) {
                Logger.Error($"Creating parent folder: {parentFolder}");
                DirAccess.MakeDirRecursiveAbsolute(parentFolder);
            }
        }

        FileAccess file = FileAccess.Open(path, flags);
        if (file == null) {
            Error error = FileAccess.GetOpenError();
            Logger.Error($"Failed to open file: {path} - {error}");
        }

        return file;
    }
}