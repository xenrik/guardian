using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ShipBuilder : Node3D {
    // Current "root" module
    [Export]
    private Module RootModule;

    [Export]
    private SubViewport SelectorViewport;

    [Export]
    private SubViewport DragViewport;

    [Export]
    private Camera3D MainCamera;

    [Export]
    private float ScrollSpeed = 1;

    [Export]
    private float MoveSpeed = 0.1f;

    [Export]
    private float RotateSpeed = 1f;

    [Export]
    private float CameraDamp = 0.75f;

    [Node]
    private Node3D ModulesRoot;

    private Camera3D dragCamera;

    private List<Tuple<Area3D, Area3D>> activeSnaps = new();
    private List<Tuple<Area3D, Area3D>> bodyCollisions = new();

    private Module selectorModule = null;
    private Module editingModule = null;
    private Node3D snapper = null;

    private Vector3 moduleStartPos;
    private Vector3 mouseDragStartPos;
    private Vector3 lastKnowGoodPos;

    private bool cameraRotating = false;
    private Vector2 mouseRotateStartPos;
    private Vector3 cameraRotMin = new(-90, -90, 0);
    private Vector3 cameraRotMax = new(90, 90, 0);

    private Vector3 cameraTarget;
    private Vector3 cameraMin = new(-10, -10, -10);
    private Vector3 cameraMax = new(10, 10, 10);

    public override void _Ready() {
        base._Ready();

        cameraTarget = MainCamera.Position;
        dragCamera = DragViewport.FindChild<Camera3D>();
        dragCamera.GlobalTransform = MainCamera.GlobalTransform;
    }

    public override void _Process(double delta) {
        base._Process(delta);

        // Keep the drag and main camera in sync
        if (dragCamera.GlobalTransform != MainCamera.GlobalTransform) {
            dragCamera.GlobalTransform = MainCamera.GlobalTransform;
        }

        HandleModuleDragging();
        HandleModuleSelector();
        HandleCamera((float)delta);
    }

    private void HandleCamera(float delta) {
        Vector3 cameraDelta = Vector3.Zero;

        // Don't move the camera if we're over the selector
        if (!SelectorViewport.GetVisibleRect().HasPoint(SelectorViewport.GetMousePosition())) {
            if (Input.IsActionJustPressed(InputKeys.Editor.MoveCamera.In)) {
                cameraDelta.Z = -1 * ScrollSpeed;
            } else if (Input.IsActionJustPressed(InputKeys.Editor.MoveCamera.Out)) {
                cameraDelta.Z = 1 * ScrollSpeed;
            }
            if (Input.IsActionPressed(InputKeys.Editor.MoveCamera.Left)) {
                cameraDelta.X = 1 * MoveSpeed;
            } else if (Input.IsActionPressed(InputKeys.Editor.MoveCamera.Right)) {
                cameraDelta.X = -1 * MoveSpeed;
            }
            if (Input.IsActionPressed(InputKeys.Editor.MoveCamera.Forward)) {
                cameraDelta.Y = -1 * MoveSpeed;
            } else if (Input.IsActionPressed(InputKeys.Editor.MoveCamera.Back)) {
                cameraDelta.Y = 1 * MoveSpeed;
            }

            if (Input.IsActionPressed(InputKeys.Editor.MoveCamera.Rotate)) {
                var mousePos = GetViewport().GetMousePosition();
                if (cameraRotating) {
                    MainCamera.RotateY((mousePos.X - mouseRotateStartPos.X) * RotateSpeed); // Yaw
                    MainCamera.RotateX((mousePos.Y - mouseRotateStartPos.Y) * RotateSpeed); // Pitch

                    var rot = MainCamera.RotationDegrees.Clamp(cameraRotMin, cameraRotMax);
                    MainCamera.RotationDegrees = rot;
                } else {
                    cameraRotating = true;
                }
                mouseRotateStartPos = mousePos;
            } else {
                cameraRotating = false;
            }
        }

        cameraTarget -= cameraDelta;
        cameraTarget = cameraTarget.Clamp(cameraMin, cameraMax);
        MainCamera.Position = MainCamera.Position.Damp(cameraTarget, CameraDamp, delta);
    }

    private void HandleModuleDragging() {
        if (Input.IsActionPressed(InputKeys.Editor.SelectModule)) {
            var mousePos = GetViewport().GetMousePosition();

            // Have to use a ray rather than MouseEntered/Exited as that doesn't work for
            // overlapping Area3Ds...
            var currentPos = MainCamera.ProjectPosition(mousePos, MainCamera.GlobalPosition.Y);
            if (Input.IsActionJustPressed(InputKeys.Editor.SelectModule) && editingModule == null) {
                // Ignore if we're in the selector viewport
                if (SelectorViewport.GetVisibleRect().HasPoint(SelectorViewport.GetMousePosition())) {
                    return;
                }

                var query = new PhysicsRayQueryParameters3D();
                query.From = MainCamera.ProjectRayOrigin(mousePos);
                query.To = query.From + MainCamera.ProjectRayNormal(mousePos) * 100;
                query.CollideWithAreas = true;
                query.CollisionMask = Layers.Module.Body;

                var result = GetWorld3D().DirectSpaceState.ProjectRay(query);
                if (result != null) {
                    editingModule = result.Collider.GetParent<Module>();
                    moduleStartPos = editingModule.GlobalPosition;
                    mouseDragStartPos = currentPos;
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
                var mouseDelta = currentPos - mouseDragStartPos;
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
            editingModule.Reparent(ModulesRoot);

            // Organise Modules
            Callable.From(() => {
                List<Module> allModules = ModulesRoot.FindChildren<Module>().ToList();
                List<Module> unattached = RootModule.OrganiseModules(allModules);

                // Any unattached modules are parented by the root
                unattached.ForEach(module => {
                    module.Reparent(ModulesRoot);
                });
            }).CallAfterPhysicsFrame();

            // If the editing module is over the selector delete it
            if (SelectorViewport.GetVisibleRect().HasPoint(SelectorViewport.GetMousePosition())) {
                editingModule.QueueFree();
            }

            // Tidy up
            editingModule = null;

            snapper.QueueFree();
            snapper = null;
            activeSnaps.Clear();
        }
    }

    private void HandleModuleSelector() {
        // Don't do anything if we're currently editing a module, or we don't have a selector module
        if (editingModule != null || selectorModule == null) {
            return;
        }

        var mousePos = GetViewport().GetMousePosition();
        var currentPos = MainCamera.ProjectPosition(mousePos, MainCamera.GlobalPosition.Y);

        var selectorCamera = SelectorViewport.GetCamera3D();
        var selectorPos = selectorCamera.ProjectPosition(mousePos, selectorCamera.GlobalPosition.Y);

        editingModule = (Module)selectorModule.Duplicate();
        ModulesRoot.AddChild(editingModule);

        // Offset so it has the right position on the main camera
        editingModule.GlobalPosition -= selectorPos - currentPos;

        moduleStartPos = editingModule.GlobalPosition;
        mouseDragStartPos = currentPos;
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

        selectorModule = null;
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);
        if (editingModule == null) {
            return;
        }

        // If the mouse is over the selector, ignore everything, and just move it (but don't update the "good" position)
        if (SelectorViewport.GetVisibleRect().HasPoint(GetViewport().GetMousePosition())) {
            if (editingModule.GetParent() != DragViewport) {
                editingModule.Reparent(DragViewport);
            }
            editingModule.GlobalPosition = snapper.GlobalPosition;
            return;
        }

        // Move back to the main viewport if needed
        if (editingModule.GetParent() != ModulesRoot) {
            editingModule.Reparent(ModulesRoot);
        }

        // If the body is colliding, move to the last know good pos        
        if (bodyCollisions.NotEmpty()) {
            editingModule.GlobalPosition = lastKnowGoodPos;
        }

        // If the snapper is causing collisions then move to the snapped position,
        // otherwise move to the snapper position        
        else if (activeSnaps.NotEmpty()) {
            var activeSnap = activeSnaps[0];
            var snapOffset = activeSnap.Item1.GlobalPosition - snapper.GlobalPosition;
            var targetSnapPos = activeSnap.Item2.GlobalPosition;

            editingModule.GlobalPosition = targetSnapPos - snapOffset;
            lastKnowGoodPos = editingModule.GlobalPosition;
        }

        // otherwise move to the snapper position
        else {
            editingModule.GlobalPosition = snapper.GlobalPosition;
            lastKnowGoodPos = editingModule.GlobalPosition;
        }
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

    private void OnModuleSelectorModuleSelected(Module module) {
        selectorModule = module;
    }

    private void OnSaveButtonPressed() {
        ModuleTree tree = ModuleTree.ToModuleTree(RootModule);
        tree.Save("user://ShipDesigns/test.json");
    }

    private void OnOrganisePressed() {
        Callable.From(() => {
            List<Module> allModules = ModulesRoot.FindChildren<Module>().ToList();
            List<Module> unattached = RootModule.OrganiseModules(allModules);

            // Any unattached modules are parented by us
            unattached.ForEach(module => {
                Logger.Debug($"Unattached module: {module.Name} being attached to the builder node");
                module.Reparent(ModulesRoot);
            });
        }).CallAfterFrame();
    }

    private void OnResetCameraButtonPressed() {
        MainCamera.Position = Vector3.Zero;
        cameraTarget = Vector3.Zero;

        MainCamera.Rotation = Vector3.Zero;
    }
}
