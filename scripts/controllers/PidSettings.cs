using Godot;

[GlobalClass]
public partial class PidSettings : Resource {
    [Export]
    public float P = 2f;
    [Export]
    public float I = 1f;
    [Export]
    public float D = 0.5f;
}
