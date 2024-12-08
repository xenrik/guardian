using Godot;
using System;

public partial class OriginRoot : Node3D {
    // The distance before we shift the origin
    public float MaxDistance = 100;

    [Singleton]
    private OriginManager originManager;

    public override void _Ready() {
        base._Ready();

        // Tell the OriginManager we are the root
        originManager.SetRoot(this);
    }

    public override void _ExitTree() {
        base._ExitTree();

        originManager.ClearRoot(this);
    }

    public override void _Process(double delta) {
        base._Process(delta);

        var camera = GetViewport().GetCamera3D();
        if (camera == null) {
            return;
        }

        var distanceDelta = (camera.GlobalPosition - GlobalPosition).LengthSquared();
        if (distanceDelta > MathF.Pow(MaxDistance, 2)) {
            var offset = camera.GlobalPosition - GlobalPosition;
            camera.GlobalPosition -= offset;
            originManager.Origin += offset;
        }
    }
}
