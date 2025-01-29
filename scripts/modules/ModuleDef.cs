using Godot;

[GlobalClass]
public partial class ModuleDef : Resource {
    [Export]
    public string ModuleId;

    [Export]
    public bool IsRootModule;
}