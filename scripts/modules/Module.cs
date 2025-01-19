using Godot;

public partial class Module : Node3D, IPersistable {
    [Export]
    [Persist]
    public string ModuleId;

    [Singleton]
    private ModuleRegistry registry;
}