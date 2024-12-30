using Godot;
using System;

public partial class OriginShiftingTest : Node {
    [Export]
    public Label originLabel;

    [Export]
    public Label boxGlobalLabel;

    [Export]
    public Label boxUniversalLabel;

    [Export]
    public Node3D box;

    public override void _Process(double delta) {
        base._Process(delta);

        var origin = DependencyInjector.GetSingleton<FloatingOrigin>();
        originLabel.Text = origin.Origin.ToString("F2");

        boxGlobalLabel.Text = box.GlobalPosition.ToString("F2");
        boxUniversalLabel.Text = box.GetUniversalPosition().ToString("F2");
    }
}
