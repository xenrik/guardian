using Godot;
using Godot.Collections;

public partial class EditorModule : Node3D {
    [Signal]
    public delegate void MouseEnteredEventHandler(EditorModule module);
    [Signal]
    public delegate void MouseExitedEventHandler(EditorModule module);
    [Signal]
    public delegate void SnapCollisionEnteredEventHandler();
    [Signal]
    public delegate void SnapCollisionExitedEventHandler();
    [Signal]
    public delegate void BodyCollisionEnteredEventHandler();
    [Signal]
    public delegate void BodyCollisionExitedEventHandler();

    public override void _Ready() {
        base._Ready();

        UpdateSnaps();
    }

    public void OnMouseEnter() {
        Logger.Debug("Enter!");
        EmitSignal(SignalName.MouseEntered, this);
    }

    public void OnMouseExit() {
        Logger.Debug("Exit!");
        EmitSignal(SignalName.MouseExited, this);
    }

    public void OnSnapCollisionEnter(Rid bodyRid, Node body, int bodyShapeIndex, int localShapeIndex) {
        //EmitSignal(SignalName.SnapCollisionEntered, this, collision);
    }

    public void OnSnapCollisionExit(Rid bodyRid, Node body, int bodyShapeIndex, int localShapeIndex) {
        // EmitSignal(SignalName.SnapCollisionExited, this, collision);
    }

    public void OnBodyCollisionEnter() {
        EmitSignal(SignalName.BodyCollisionEntered, this);
    }

    public void OnBodyCollisionExit() {
        EmitSignal(SignalName.BodyCollisionExited, this);
    }

    /**
     * Update the collection layers on the snaps, based on their current rotation.
     * Note, we expect snaps to have a rotation that matches one of the three sides of a triangle
     * and we then work out if it's "north" or "south" based on the snaps relative position to its
     * parent
     */
    private void UpdateSnaps() {
        Array<Node3D> snapNodes = this.FindChildren<Node3D>("Snap?");
        Logger.Debug($"{Name} has {snapNodes.Count} snaps");

        Vector3 modulePos = GlobalPosition;        
        foreach (Node3D node in snapNodes) {
            string suffix = "";
            Vector3 nodePos = node.GlobalPosition;

            float dz = nodePos.Z - modulePos.Z;
            float dx = nodePos.X - modulePos.X;
            if (Mathf.Abs(dz) > Mathf.Epsilon) {
                suffix += dz > 0 ? "S" : "N";
            }
            if (Mathf.Abs(dx) > Mathf.Epsilon) {
                suffix += dx > 0 ? "E" : "W";
            }

            Logger.Debug($"   Snap: {node.Name} - Suffix: {suffix}");
        }
    }
}
