using Godot;

public enum ModuleType {
    Bulkhead,
    Cockpit
}

[GlobalClass]
public partial class ModuleDef : Resource {
    [Export]
    public string ModuleId;

    [Export]
    public ModuleType Type;

    [Export]
    public Vector3 SelectorPivot;

    [Export]
    public Vector3 SelectorAdditionalOffset;
}