using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShipBuilder : Node3D {
    // Current "root" module
    [Export]
    private EditorModule rootModule;

    // Dragging Support
    private List<EditorModule> activeModules = new();
    private List<Tuple<Area3D, Area3D>> activeSnaps = new();

    private EditorModule editingModule = null;
    private EditorModule snapper = null;

    private Vector3 moduleStartPos;
    private Vector3 mouseStartPos;
    private Vector3 targetPosition;

    public override void _Process(double delta) {
        base._Process(delta);

        if (Input.IsActionPressed(InputKeys.Editor.SelectModule)) {
            var camera = GetViewport().GetCamera3D();
            var currentPos = camera.ProjectPosition(GetViewport().GetMousePosition(), camera.GlobalPosition.Y);

            if (editingModule == null && activeModules.Count > 0) {
                // Start Editing
                editingModule = activeModules[0];
                moduleStartPos = editingModule.GlobalPosition;
                mouseStartPos = currentPos;

                // Create the invisible snapper
                snapper = (EditorModule)editingModule.Duplicate(~(int)DuplicateFlags.Signals);
                snapper.Name = editingModule.Name + "_snapper";
                AddChild(snapper);

                snapper.FindChildren<MeshInstance3D>().ForEach(mesh => mesh.Visible = false);
                snapper.SnapEntered += OnSnapEntered;
                snapper.SnapExited += OnSnapExited;

                // Disable the snaps on the current module
                editingModule.FindChildren<Area3D>("Snap?").ForEach(snap => snap.ProcessMode = ProcessModeEnum.Disabled);
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
            editingModule.FindChildren<Area3D>("Snap?").ForEach(snap => snap.ProcessMode = ProcessModeEnum.Always);

            // Connect all modules to the root if they are not already
            foreach (var module in this.FindChildren<Module>()) {
                if (module == rootModule) {
                    continue;
                }

                if (!module.HasParent(rootModule)) {
                    module.Reparent(rootModule);
                }
            }

            // Organise Modules
            Callable.From(() => {
                List<Module> unattached = rootModule.OrganiseModules();

                // Any unattached modules are parented by us
                unattached.ForEach(module => {
                    Logger.Debug($"Unnattached module: {module.Name} being attached to the builder node");
                    module.Reparent(this);
                });
            }).CallAfterPhysicsUpdate();

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

    private void OnModuleEntered(EditorModule module) {
        if (!activeModules.Contains(module)) {
            activeModules.Add(module);
        }
    }

    private void OnModuleExited(EditorModule module) {
        activeModules.Remove(module);
    }

    private void OnSnapEntered(Area3D snap, Area3D otherSnap) {
        if (snapper == null || !snap.HasParent(snapper)) {
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

    private void OnSaveButtonPressed() {
        ModuleTree tree = ModuleTree.ToModuleTree(rootModule);
        tree.Save("user://ShipDesigns/test.json");
    }
}
