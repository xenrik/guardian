using System.Linq;
using System.Collections.Generic;
using Godot;

public partial class Module : Node3D {
    [Signal]
    public delegate void SnapEnteredEventHandler(Area3D snap, Area3D otherSnap);
    [Signal]
    public delegate void SnapExitedEventHandler(Area3D snap, Area3D otherSnap);
    [Signal]
    public delegate void BodyCollisionEnteredEventHandler();
    [Signal]
    public delegate void BodyCollisionExitedEventHandler();

    [Export]
    public ModuleDef ModuleDef;

    [Singleton]
    private ModuleRegistry registry;

    private HashSet<SnapCollision> touchingSnaps = new();

    public override void _Ready() {
        base._Ready();

        UpdateSnaps();
    }

    public void OnSnapEnter(Rid areaRid, Area3D area, int areaShapeIndex, int localShapeIndex, string snapName) {
        SnapCollision collision = new SnapCollision(this, snapName, area, areaRid);
        if (collision.IsValid()) {
            touchingSnaps.Add(collision);
            EmitSignal(SignalName.SnapEntered, collision.Snap, collision.OtherSnap);
        }
    }

    public void OnSnapExit(Rid areaRid, Area3D area, int areaShapeIndex, int localShapeIndex, string snapName) {
        SnapCollision collision = new SnapCollision(this, snapName, area, areaRid);

        // Remove even if it's not valid!
        touchingSnaps.Remove(collision);

        if (collision.IsValid()) {
            EmitSignal(SignalName.SnapExited, collision.Snap, collision.OtherSnap);
        }
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
        Godot.Collections.Array<Area3D> snapNodes = this.FindChildren<Area3D>("Snap?");
        // Logger.Debug($"{Name} has {snapNodes.Count} snaps");

        Vector3 modulePos = GlobalPosition;
        foreach (Area3D node in snapNodes) {
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

            // Logger.Debug($"   Snap: {node.Name} - Layer: {LayerConstants.ToString(node.CollisionLayer)} - Mask: {LayerConstants.ToString(node.CollisionMask)}");
        }
    }

    /**
     * Reorganise child nodes so that they are parented by the shorted path to this node.
     * Returns a list of any modules which are now not attached. This relies on collision detection
     * and therefore if modules have been moved you should wait for a physics update to get accurate results
     */
    public List<Module> OrganiseModules() {
        Logger.Debug("Optimising!");

        // First collect all nodes and reset their depth
        Godot.Collections.Array<Module> allModules = this.FindChildren<Module>();
        Dictionary<Module, int> depth = new();
        Dictionary<Module, Module> parent = new();

        foreach (Module module in allModules) {
            depth[module] = -1;
            parent[module] = null;
        }

        depth[this] = 0;

        // Calculate the best depth and best parent
        var updated = true;
        var loops = 0;
        while (updated) {
            updated = false;

            foreach (Module module in allModules) {
                int newDepth = -1;
                Module newParent = null;

                foreach (SnapCollision collision in module.touchingSnaps) {
                    int otherDepth = depth[collision.OtherModule];
                    if (otherDepth == -1) {
                        continue;
                    } else if (newDepth == -1 || otherDepth < newDepth) {
                        newDepth = otherDepth;
                        newParent = collision.OtherModule;
                    }
                }

                if (newDepth != depth[module]) {
                    depth[module] = newDepth;
                    parent[module] = newParent;
                    updated = true;
                }
            }

            if (loops++ > 1000) {
                Logger.Error("LOOOOP OVERFLOW!");
                break;
            }
        }

        // Reparent to the best parents and collect unattached modules
        List<Module> unattachedModules = new();
        foreach (Module module in allModules) {
            var newParent = parent[module];
            if (newParent != module.GetParent()) {
                if (newParent != null) {
                    Logger.Debug($"module: {module.Name} is now parented by: {newParent.Name}");
                    module.Reparent(newParent);
                } else {
                    Logger.Debug($"module: {module.Name} is unnattached");
                    unattachedModules.Add(module);
                }
            } else {
                Logger.Debug($"module: {module.Name} is unmodified");
            }
        }

        Logger.Debug("Finished Optimising!");
        return unattachedModules;
    }
}