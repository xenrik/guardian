
using Godot;

[GlobalClass]
public partial class Variable : Resource {
    [Export]
    public string Name;

    [Export]
    public string Expression;
}
