using Godot;

[GlobalClass]
public partial class ModuleDef : Resource {
    [Export]
    public string ModuleId;

    [Export]
    public bool IsRootModule;

    [Export]
    public Vector3 SelectorPivot;

    [Export]
    public Vector3 SelectorAdditionalOffset;
}