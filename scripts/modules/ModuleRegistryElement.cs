using Godot;

[GlobalClass]
public partial class ModuleRegistryElement : Resource {
    [Export]
    public string ModuleId;

    [Export]
    public PackedScene GameScene;

    [Export]
    public PackedScene EditorScene;
}