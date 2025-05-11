
using Godot.Collections;

/// <summary>
/// Used to upgrade module trees to the current version
/// </summary>
public class ModuleTreeUpgrader {
    private ModuleTreeUpgrader() { }

    public static ModuleTreeUpgrader GetUpgrader(string _) {
        return new ModuleTreeUpgrader();
    }

    public Dictionary Upgrade(Dictionary data) {
        return data;
    }
}