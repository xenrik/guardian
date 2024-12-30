using Godot;

[GlobalClass]
public partial class Expression : Resource {
    [Export]
    public string Name;

    [Export]
    public string Value;

    public string Resolve(Node context) {
        return "";
    }
}