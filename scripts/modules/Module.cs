using Godot;

public partial class Module : Node3D {
    [Export]
    public string ModuleId;

    [Singleton]
    private ModuleRegistry registry;
}