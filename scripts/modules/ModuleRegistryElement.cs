using Godot;

[GlobalClass]
public partial class ModuleRegistryElement : Resource {
    [Export]
    public ModuleDef Definition;

    [Export]
    public PackedScene GameScene;

    [Export]
    public PackedScene EditorScene;
}