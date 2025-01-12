using Godot;
using Godot.Collections;

public partial class EditorModule : Node3D {
    [Signal]
    public delegate void MouseEnteredEventHandler(EditorModule module);
    [Signal]
    public delegate void MouseExitedEventHandler(EditorModule module);
    [Signal]
    public delegate void SnapEnteredEventHandler(Area3D snap, Area3D otherSnap);
    [Signal]
    public delegate void SnapExitedEventHandler(Area3D snap, Area3D otherSnap);
    [Signal]
    public delegate void BodyCollisionEnteredEventHandler();
    [Signal]
    public delegate void BodyCollisionExitedEventHandler();

    public override void _Ready() {
        base._Ready();

        UpdateSnaps();
    }

    public void OnMouseEnter() {
        EmitSignal(SignalName.MouseEntered, this);
    }

    public void OnMouseExit() {
        EmitSignal(SignalName.MouseExited, this);
    }

    public void OnSnapEnter(Rid areaRid, Area3D area, int areaShapeIndex, int localShapeIndex, string snapName) {
        EmitSignal(SignalName.SnapEntered, FindChild(snapName), area);
    }

    public void OnSnapExit(Rid areaRid, Area3D area, int areaShapeIndex, int localShapeIndex, string snapName) {
        EmitSignal(SignalName.SnapExited, FindChild(snapName), area);
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
        Array<Area3D> snapNodes = this.FindChildren<Area3D>("Snap?");
        Logger.Debug($"{Name} has {snapNodes.Count} snaps");

        Vector3 modulePos = GlobalPosition;
        foreach (Area3D node in snapNodes) {
            string suffix = "";
            Vector3 nodePos = node.GlobalPosition;

            float dz = nodePos.Z - modulePos.Z;
            float dx = nodePos.X - modulePos.X;

            dz = Mathf.Abs(dz) > Mathf.Epsilon ? Mathf.Sign(dz) : 0;
            dx = Mathf.Abs(dx) > Mathf.Epsilon ? Mathf.Sign(dx) : 0;

            if (dz > 0 && dx > 0) {
                node.CollisionLayer = LayerConstants.Snap_NE.Mask;
                node.CollisionMask = LayerConstants.Snap_SW.Mask;
            } else if (dz < 0 && dx > 0) {
                node.CollisionLayer = LayerConstants.Snap_NW.Mask;
                node.CollisionMask = LayerConstants.Snap_SE.Mask;
            } else if (dz > 0 && dx < 0) {
                node.CollisionLayer = LayerConstants.Snap_SE.Mask;
                node.CollisionMask = LayerConstants.Snap_NW.Mask;
            } else if (dz < 0 && dx < 0) {
                node.CollisionLayer = LayerConstants.Snap_SW.Mask;
                node.CollisionMask = LayerConstants.Snap_NE.Mask;
            } else if (dz > 0 && dx == 0) {
                node.CollisionLayer = LayerConstants.Snap_E.Mask;
                node.CollisionMask = LayerConstants.Snap_W.Mask;
            } else if (dz < 0 && dx == 0) {
                node.CollisionLayer = LayerConstants.Snap_W.Mask;
                node.CollisionMask = LayerConstants.Snap_E.Mask;
            } else {
                Logger.Error($"{Name} has a snap {node.Name} which doesn't seem to have a valid rotation! (dz: {dz} - dx: {dx})");
                continue;
            }

            Logger.Debug($"   Snap: {node.Name} - Layer: {LayerConstants.ToString(node.CollisionLayer)} - Mask: {LayerConstants.ToString(node.CollisionMask)}");
        }
    }
}
