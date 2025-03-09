using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ShipBuilder : Node3D {
    // Current "root" module
    [Export]
    private Module rootModule;

    [Node]
    private Label DebugInfo;

    [Node]
    private Node3D ModulesRoot;

    // Dragging Support
    private List<Tuple<Area3D, Area3D>> activeSnaps = new();
    private List<Tuple<Area3D, Area3D>> bodyCollisions = new();

    private Module editingModule = null;
    private Node3D snapper = null;

    private Vector3 moduleStartPos;
    private Vector3 mouseStartPos;
    private Vector3 lastKnowGoodPos;

    private double debugUpdate = 0;
    private uint lastDebugHash = 0;

    public override void _Process(double delta) {
        base._Process(delta);

        if (Input.IsActionPressed(InputKeys.Editor.SelectModule)) {
            var camera = GetViewport().GetCamera3D();
            var mousePos = GetViewport().GetMousePosition();

            // Have to use a ray rather than MouseEntered/Exited as that doesn't work for
            // overlapping Area3Ds...
            //var space = 
            var currentPos = camera.ProjectPosition(mousePos, camera.GlobalPosition.Y);
            if (editingModule == null) {
                var query = new PhysicsRayQueryParameters3D();
                query.From = camera.ProjectRayOrigin(mousePos);
                query.To = query.From + camera.ProjectRayNormal(mousePos) * 100;
                query.CollideWithAreas = true;
                query.CollisionMask = Layers.Module.Body;

                var result = GetWorld3D().DirectSpaceState.ProjectRay(query);
                if (result != null) {
                    editingModule = result.Collider.GetParent<Module>();
                    moduleStartPos = editingModule.GlobalPosition;
                    mouseStartPos = currentPos;
                    lastKnowGoodPos = moduleStartPos;

                    // Create the snapper
                    snapper = new Node3D();
                    AddChild(snapper);
                    snapper.GlobalTransform = editingModule.GlobalTransform;

                    activeSnaps.Clear();
                    bodyCollisions.Clear();

                    editingModule.FindChildren(Groups.Module.Snap.Filter<Area3D>()).ForEach(node => {
                        var snapperArea = (Area3D)node.Duplicate(0);
                        snapper.AddChild(snapperArea);
                        snapperArea.GlobalTransform = node.GlobalTransform;

                        snapperArea.Connect(Area3D.SignalName.AreaShapeEntered, Callable.From<Rid, Area3D, int, int>((_, area, _, _) => {
                            OnSnapEntered(snapperArea, area);
                        }));
                        snapperArea.Connect(Area3D.SignalName.AreaShapeExited, Callable.From<Rid, Area3D, int, int>((_, area, _, _) => {
                            OnSnapExited(snapperArea, area);
                        }));
                    });
                    editingModule.FindChildren(Groups.Module.Body.Filter<Area3D>()).ForEach(node => {
                        var snapperArea = (Area3D)node.Duplicate(0);
                        snapper.AddChild(snapperArea);
                        snapperArea.GlobalTransform = node.GlobalTransform;

                        snapperArea.Connect(Area3D.SignalName.AreaShapeEntered, Callable.From<Rid, Area3D, int, int>((_, area, _, _) => {
                            OnBodyEntered(snapperArea, area);
                        }));
                        snapperArea.Connect(Area3D.SignalName.AreaShapeExited, Callable.From<Rid, Area3D, int, int>((_, area, _, _) => {
                            OnBodyExited(snapperArea, area);
                        }));
                    });

                    snapper.Name = editingModule.Name + "_snapper";

                    // Disable the snaps and body on the current module
                    editingModule.FindChildren(Groups.Module.Snap.Filter<Area3D>()).ForEach(snap => snap.ProcessMode = ProcessModeEnum.Disabled);
                    editingModule.FindChildren(Groups.Module.Body.Filter<Area3D>()).ForEach(snap => snap.ProcessMode = ProcessModeEnum.Disabled);
                }
            } else if (editingModule != null) {
                // Drag
                var mouseDelta = currentPos - mouseStartPos;
                var newPos = moduleStartPos + mouseDelta;
                newPos.Y = moduleStartPos.Y;

                // Reposition the snapper
                snapper.GlobalPosition = newPos;
            }
        } else if (editingModule != null) {
            // End Editing

            // Reenable the snaps on the current module
            editingModule.FindChildren(Groups.Module.Snap.Filter<Area3D>()).ForEach(snap => snap.ProcessMode = ProcessModeEnum.Always);
            editingModule.FindChildren(Groups.Module.Body.Filter<Area3D>()).ForEach(snap => snap.ProcessMode = ProcessModeEnum.Always);

            // Make sure all the modules are linked to the root
            ModulesRoot.FindChildren<Module>().ForEach(module => module.Reparent(ModulesRoot));

            // Organise Modules
            Callable.From(() => {
                List<Module> allModules = ModulesRoot.FindChildren<Module>().ToList();
                List<Module> unattached = rootModule.OrganiseModules(allModules);

                // Any unattached modules are parented by the root
                unattached.ForEach(module => {
                    module.Reparent(ModulesRoot);
                });
            }).CallAfterPhysicsFrame();

            // Tidy up
            editingModule = null;

            snapper.QueueFree();
            snapper = null;
            activeSnaps.Clear();
        }

        debugUpdate -= delta;
        if (debugUpdate < 0) {
            debugUpdate = 0.1;
            UpdateDebug();
        }
    }

    private void OnOrganisePressed() {
        Callable.From(() => {
            List<Module> allModules = ModulesRoot.FindChildren<Module>().ToList();
            List<Module> unattached = rootModule.OrganiseModules(allModules);

            // Any unattached modules are parented by us
            unattached.ForEach(module => {
                Logger.Debug($"Unattached module: {module.Name} being attached to the builder node");
                module.Reparent(ModulesRoot);
            });
        }).CallAfterFrame();
    }

    private void UpdateDebug() {
        var debug = "";
        foreach (var childName in new string[] { "red", "green", "blue" }) {
            var child = ModulesRoot.FindChild(childName);
            debug += $"{childName} Parent: {child.GetParent().Name}\n";

            List<Area3D> snaps = new();
            child.WalkTree(node => {
                if (node.IsInGroup(Groups.Module.Snap)) {
                    snaps.Add((Area3D)node);
                    return TreeWalker.Result.SKIP_CHILDREN;
                } else if (node is Module) {
                    return TreeWalker.Result.SKIP_CHILDREN;
                } else {
                    return TreeWalker.Result.RECURSE;
                }
            });
            foreach (var snap in snaps) {
                foreach (var overlap in snap.GetOverlappingAreas()) {
                    var module = overlap.FindParent<Module>();
                    debug += $"   {snap.Name} - Overlaps: {module?.Name}\n";
                }
            }
        }

        /*
        if (debug.Hash() != lastDebugHash) {
            Logger.Debug(debug);
            lastDebugHash = debug.Hash();
        }
        */

        DebugInfo.Text = debug;
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        if (editingModule == null) {
            return;
        }

        // If the snapper is causing collisions then move to the snapped position,
        // otherwise move to the snapper position        
        if (bodyCollisions.NotEmpty()) {
            // If the body is colliding, move to the last know good pos
            editingModule.GlobalPosition = lastKnowGoodPos;
        } else if (activeSnaps.NotEmpty()) {
            var activeSnap = activeSnaps[0];
            var snapOffset = activeSnap.Item1.GlobalPosition - snapper.GlobalPosition;
            var targetSnapPos = activeSnap.Item2.GlobalPosition;

            editingModule.GlobalPosition = targetSnapPos - snapOffset;
        } else {
            editingModule.GlobalPosition = snapper.GlobalPosition;
        }

        lastKnowGoodPos = editingModule.GlobalPosition;
    }

    private void OnSnapEntered(Area3D snap, Area3D otherSnap) {
        if (otherSnap.HasParent(snapper)) {
            return;
        }

        var activeSnap = new Tuple<Area3D, Area3D>(snap, otherSnap);
        if (!activeSnaps.Contains(activeSnap)) {
            activeSnaps.Add(activeSnap);
        }
    }

    private void OnSnapExited(Area3D snap, Area3D otherSnap) {
        var activeSnap = new Tuple<Area3D, Area3D>(snap, otherSnap);
        activeSnaps.Remove(activeSnap);
    }

    private void OnBodyEntered(Area3D body, Area3D otherBody) {
        if (otherBody.HasParent(snapper)) {
            return;
        }

        var collision = new Tuple<Area3D, Area3D>(body, otherBody);
        if (!bodyCollisions.Contains(collision)) {
            bodyCollisions.Add(collision);
        }
    }

    private void OnBodyExited(Area3D body, Area3D otherBody) {
        var collision = new Tuple<Area3D, Area3D>(body, otherBody);
        bodyCollisions.Remove(collision);
    }

    private void OnSaveButtonPressed() {
        ModuleTree tree = ModuleTree.ToModuleTree(rootModule);
        tree.Save("user://ShipDesigns/test.json");
    }
}
