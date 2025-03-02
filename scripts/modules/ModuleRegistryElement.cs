using Godot;

[GlobalClass]
public partial class ModuleRegistryElement : Resource {
    [Export]
    public ModuleDef Definition;

    [Export]
    public PackedScene Scene;
}