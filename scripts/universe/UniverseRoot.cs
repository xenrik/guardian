using Godot;
using System;

public partial class OriginRoot : Node3D {
    // The distance before we shift the origin
    public float MaxDistance = 100;

    [Singleton]
    private UniverseManager universeManager;

    private Camera3D lastCamera;

    public override void _Ready() {
        base._Ready();

        // Tell the UniverseManager we are the root
        universeManager.SetRoot(this);
    }

    public override void _ExitTree() {
        base._ExitTree();

        universeManager.ClearRoot(this);
    }

    public override void _PhysicsProcess(double delta) {
        base._Process(delta);

        var camera = GetViewport().GetCamera3D();
        if (camera == null) {
            return;
        } else if (lastCamera != camera) {
            // Validate the camera
            lastCamera = camera;
            if (!camera.HasParent(this)) {
                GD.Print("WARNING - The camera is not parented by the universe origin!");
            }
        }

        var distanceDelta = (camera.GlobalPosition - GlobalPosition).LengthSquared();
        if (distanceDelta > MathF.Pow(MaxDistance, 2)) {
            var offset = camera.GlobalPosition - GlobalPosition;
            camera.GlobalPosition -= offset;
            universeManager.Origin += offset;
        }
    }
}
