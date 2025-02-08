using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ShipBuilder : Node3D {
    // Current "root" module
    [Export]
    private Module rootModule;

    // Dragging Support
    private List<Tuple<Area3D, Area3D>> activeSnaps = new();

    private Module editingModule = null;
    private Module snapper = null;

    private Vector3 moduleStartPos;
    private Vector3 mouseStartPos;

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

                    // Create the invisible snapper
                    snapper = (Module)editingModule.Duplicate();
                    AddChild(snapper);

                    snapper.Name = editingModule.Name + "_snapper";
                    snapper.GlobalTransform = editingModule.GlobalTransform;

                    snapper.FindChildren<MeshInstance3D>().ForEach(mesh => mesh.Visible = false);
                    snapper.SnapEntered += OnSnapEntered;
                    snapper.SnapExited += OnSnapExited;

                    // Disable the snaps on the current module
                    editingModule.FindChildren(Groups.Module.Snap.Filter<Area3D>()).ForEach(snap => snap.ProcessMode = ProcessModeEnum.Disabled);
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

            // Reparent all nodes to us so we can properly detect collisions between modules
            this.FindChildren<Module>().ToList().ForEach(module => module.Reparent(this));

            // Organise Modules
            Logger.Debug("End Editing");
            Callable.From(() => {
                List<Module> allModules = this.FindChildren<Module>().ToList();
                List<Module> unattached = rootModule.OrganiseModules(allModules);

                // Any unattached modules are parented by us
                unattached.ForEach(module => {
                    Logger.Debug($"Unnattached module: {module.Name} being attached to the builder node");
                    module.Reparent(this);
                });
            }).CallAfterFrame();

            // Tidy up
            editingModule = null;

            snapper.QueueFree();
            snapper = null;
            activeSnaps.Clear();
        }
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        if (editingModule == null) {
            return;
        }

        // If the snapper is causing collisions then move to the snapped position,
        // otherwise move to the snapper position        
        if (activeSnaps.NotEmpty()) {
            var activeSnap = activeSnaps[0];
            var snapOffset = activeSnap.Item1.GlobalPosition - snapper.GlobalPosition;
            var targetSnapPos = activeSnap.Item2.GlobalPosition;

            editingModule.GlobalPosition = targetSnapPos - snapOffset;
        } else {
            editingModule.GlobalPosition = snapper.GlobalPosition;
        }
    }

    private void OnSnapEntered(Area3D snap, Area3D otherSnap) {
        var activeSnap = new Tuple<Area3D, Area3D>(snap, otherSnap);
        if (!activeSnaps.Contains(activeSnap)) {
            activeSnaps.Add(activeSnap);
        }
    }

    private void OnSnapExited(Area3D snap, Area3D otherSnap) {
        var activeSnap = new Tuple<Area3D, Area3D>(snap, otherSnap);
        activeSnaps.Remove(activeSnap);
    }

    private void OnSaveButtonPressed() {
        ModuleTree tree = ModuleTree.ToModuleTree(rootModule);
        tree.Save("user://ShipDesigns/test.json");
    }
}
