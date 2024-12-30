using Godot;
using System;
using System.Linq;

[Singleton]
public partial class FloatingOrigin : Node3D {
    [Signal]
    public delegate void OriginChangedEventHandler();

    // The distance before we shift the origin
    private float _maxDistance = 100;
    private float maxDistanceSq;

    [Export]
    public float MaxDistance {
        get {
            return _maxDistance;
        }
        private set {
            this._maxDistance = value;
            this.maxDistanceSq = Mathf.Pow(value, 2);
        }
    }

    private Vector3d _origin;
    public Vector3d Origin {
        get {
            return _origin;
        }

        set {
            this._origin = value;
            EmitSignal(SignalName.OriginChanged);
        }
    }

    public override void _Ready() {
        base._Ready();

        // Make sure the squared distance is correct
        MaxDistance = MaxDistance;
    }

    public override void _PhysicsProcess(double delta) {
        base._Process(delta);

        var camera = GetViewport().GetCamera3D();
        if (camera == null) {
            return;
        }

        var distanceDelta = (camera.GlobalPosition - GlobalPosition).LengthSquared();
        if (distanceDelta > maxDistanceSq) {
            var offset = camera.GlobalPosition - GlobalPosition;

            // Update the origin
            var oldOrigin = Origin;
            Origin += offset;
            //GD.Print($"Origin Moved, old origin: {oldOrigin}, new origin: {Origin}, offset: {offset}");

            // Iterate all the children
            this.WalkTree(node => {
                // If the node wants to handle the origin shift, let it
                if (node is FloatingOriginHandler) {
                    return ((FloatingOriginHandler)node).OnFloatingOriginUpdated(oldOrigin, Origin);
                }

                // Otherwise, if we can do it ourselves
                else if (node is Node3D) {
                    var node3D = (Node3D)node;
                    node3D.GlobalPosition -= offset;

                    // Don't need to recurse further
                    return false;
                }

                // Recurse into children
                return true;
            });

            // Finally signal anyone else interested (have to downcast to Vector3!)
            EmitSignal(SignalName.OriginChanged, (Vector3)oldOrigin, (Vector3)Origin);
        }
    }
}
