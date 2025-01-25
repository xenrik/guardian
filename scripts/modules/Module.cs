using Godot;

public partial class Module : Node3D {
    [Export]
    public ModuleDef ModuleDef;

    [Singleton]
    private ModuleRegistry registry;
}