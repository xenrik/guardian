using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Module : Node3D {
    [Export]
    public ModuleDef ModuleDef;

    [Singleton]
    private ModuleRegistry registry;

    private List<BodyCollision> shipBodyCollisions = new();
    private HashSet<SnapCollision> touchingSnaps = new();

    public override void _Ready() {
        base._Ready();

        // Prepare the collisions for adding to our parent rigid/static body
        this.WalkTree(node => {
            if (node.IsInGroup(Groups.Module.Body)) {
                node.GetChildren<CollisionShape3D>().ForEach(collision => shipBodyCollisions.Add(new BodyCollision(collision)));
                return TreeWalker.Result.SKIP_CHILDREN;
            } else if (node is Module) {
                return TreeWalker.Result.SKIP_CHILDREN;
            }

            return TreeWalker.Result.RECURSE;
        });

        UpdateBody();
        UpdateSnaps();
    }

    public override void _Notification(int what) {
        switch ((long)what) {
            case Node.NotificationUnparented:
            case Node.NotificationParented:
                UpdateBody();
                break;
        }
    }

    private void UpdateBody() {
        // Find the nearest body
        PhysicsBody3D body = this.FindParent<PhysicsBody3D>();
        shipBodyCollisions.ForEach(collision => {
            Callable.From(() => {
                collision.Reparent(body);
            }).CallDeferred();
        });
    }

    /**
     * Update the collection layers on the snaps, based on their current rotation.
     * Note, we expect snaps to have a rotation that matches one of the three sides of a triangle
     * and we then work out if it's "north" or "south" based on the snaps relative position to its
     * parent
     */
    private void UpdateSnaps() {
        Godot.Collections.Array<Area3D> snapNodes = this.FindChildren(Groups.Module.Snap.Filter<Area3D>());
        // Logger.Debug($"{Name} has {snapNodes.Count} snaps");

        Vector3 modulePos = GlobalPosition;
        foreach (Area3D node in snapNodes) {
            Vector3 nodePos = node.GlobalPosition;

            float dz = nodePos.Z - modulePos.Z;
            float dx = nodePos.X - modulePos.X;

            dz = Mathf.Abs(dz) > Mathf.Epsilon ? Mathf.Sign(dz) : 0;
            dx = Mathf.Abs(dx) > Mathf.Epsilon ? Mathf.Sign(dx) : 0;

            if (dz > 0 && dx > 0) {
                node.CollisionLayer = Layers.Module.Snap.NE;
                node.CollisionMask = Layers.Module.Snap.SW;
            } else if (dz < 0 && dx > 0) {
                node.CollisionLayer = Layers.Module.Snap.NW;
                node.CollisionMask = Layers.Module.Snap.SE;
            } else if (dz > 0 && dx < 0) {
                node.CollisionLayer = Layers.Module.Snap.SE;
                node.CollisionMask = Layers.Module.Snap.NW;
            } else if (dz < 0 && dx < 0) {
                node.CollisionLayer = Layers.Module.Snap.SW;
                node.CollisionMask = Layers.Module.Snap.NE;
            } else if (dz > 0 && dx == 0) {
                node.CollisionLayer = Layers.Module.Snap.E;
                node.CollisionMask = Layers.Module.Snap.W;
            } else if (dz < 0 && dx == 0) {
                node.CollisionLayer = Layers.Module.Snap.W;
                node.CollisionMask = Layers.Module.Snap.E;
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
    public List<Module> OrganiseModules(List<Module> allModules) {
        Logger.Debug("Organising Modules!");

        // First collect all nodes and reset their depth
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
            Logger.Debug("Loop Starting");

            updated = false;

            foreach (Module module in allModules) {
                if (module == this) {
                    continue;
                }

                int newDepth = -1;
                Module newParent = null;

                Logger.Debug($"Checking collisions for module {module.Name}");
                foreach (Area3D snap in module.FindChildren(Groups.Module.Snap.Filter<Area3D>())) {
                    var overlaps = snap.GetOverlappingAreas();
                    foreach (Area3D other in overlaps) {
                        if (other.IsInGroup(Groups.Module.Snap)) {
                            Module otherModule = other.FindParent<Module>();
                            Logger.Debug($"   Has a link from snap: {snap.Name} to module: {otherModule.Name}");

                            int otherDepth = depth[otherModule];
                            if (otherDepth == -1) {
                                Logger.Debug($"   Other module has no depth yet");
                                continue;
                            } else if (newDepth == -1 || otherDepth < newDepth) {
                                newDepth = otherDepth + 1;
                                newParent = otherModule;

                                Logger.Debug($"   Setting depth to: " + newDepth);
                            }
                        }
                    }
                }

                if (newDepth != depth[module]) {
                    depth[module] = newDepth;
                    parent[module] = newParent;
                    updated = true;
                }
            }

            if (loops++ > 1000) {
                Logger.Error("LOOP OVERFLOW!");
                break;
            }
        }

        // Reparent to the best parents and collect unattached modules
        List<Module> unattachedModules = new();
        foreach (Module module in allModules) {
            if (module == this) {
                continue;
            }

            var newParent = parent[module];
            if (newParent != module.GetParent()) {
                if (newParent != null) {
                    Logger.Debug($"module: {module.Name} is now parented by: {newParent.Name}");
                    module.Reparent(newParent);
                } else {
                    Logger.Debug($"module: {module.Name} is unattached");
                    unattachedModules.Add(module);
                }
            } else {
                Logger.Debug($"module: {module.Name} is unmodified (newParent: {newParent.Name} - currentParent: {module.GetParent().Name})");
            }
        }

        Logger.Debug("Finished Optimising!");
        return unattachedModules;
    }

    private class BodyCollision {
        public CollisionShape3D original;
        public CollisionShape3D bodyCollision;

        public BodyCollision(CollisionShape3D original) {
            this.original = original;
            this.bodyCollision = (CollisionShape3D)original.Duplicate(0);
        }

        public void Reparent(PhysicsBody3D parent) {
            if (parent == bodyCollision.GetParent()) {
                return;
            }

            if (bodyCollision.GetParent() != null) {
                bodyCollision.GetParent().RemoveChild(bodyCollision);
            }

            if (parent != null) {
                parent.AddChild(bodyCollision);
                bodyCollision.GlobalTransform = original.GlobalTransform;
            }
        }
    }
}