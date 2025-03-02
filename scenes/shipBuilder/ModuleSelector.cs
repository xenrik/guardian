using System;
using Godot;

public partial class ModuleSelector : Node3D {
    [Export]
    private Node3D Origin1;
    [Export]
    private Node3D Origin2;

    [Singleton]
    private ModuleRegistry registry;

    public override void _Ready() {
        var currentPosition = Origin1.GlobalPosition;
        var delta = Origin2.GlobalPosition - currentPosition;

        foreach (var elem in registry.Elements) {
            var node = elem.Scene.Instantiate<Node3D>();
            AddChild(node);

            node.GlobalPosition = currentPosition;
            currentPosition += delta;
        }
    }
}
