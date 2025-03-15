using System;
using Godot;

public partial class ModuleSelector : Node3D {
    [Signal]
    public delegate void ModuleSelectedEventHandler(Module module);

    [Export]
    private Node3D Origin1;
    [Export]
    private Node3D Origin2;

    [Export]
    private float ScrollSpeed = 1;

    [Export]
    private float ScrollDamp = 0.75f;

    [Singleton]
    private ModuleRegistry registry;

    [Node]
    private Camera3D SelectorCamera;

    private Vector3 cameraMin;
    private Vector3 cameraMax;
    private Vector3 cameraTarget;

    public override void _Ready() {
        base._Ready();

        var currentPosition = Origin1.GlobalPosition;
        var lastPosition = currentPosition;
        var cameraOffset = currentPosition - SelectorCamera.GlobalPosition;
        var delta = Origin2.GlobalPosition - currentPosition;

        foreach (var elem in registry.Elements) {
            var node = elem.Scene.Instantiate<Module>();
            AddChild(node);

            node.GlobalPosition = currentPosition + node.ModuleDef.SelectorPivot;
            lastPosition = currentPosition;
            currentPosition += delta + node.ModuleDef.SelectorAdditionalOffset;
        }

        cameraTarget = SelectorCamera.Position;
        cameraMin = cameraTarget;
        cameraMax = lastPosition - cameraOffset;
    }

    public override void _Process(double delta) {
        base._Process(delta);

        int axis = 0;
        if (GetViewport().GetVisibleRect().HasPoint(GetViewport().GetMousePosition())) {
            if (Input.IsActionJustReleased(InputKeys.Editor.Selector.ScrollDown)) {
                axis = -1;
            } else if (Input.IsActionJustReleased(InputKeys.Editor.Selector.ScrollUp)) {
                axis = 1;
            }
        }

        cameraTarget.X += ScrollSpeed * axis;
        cameraTarget = cameraTarget.Clamp(cameraMin, cameraMax);
        SelectorCamera.Position = SelectorCamera.Position.Damp(cameraTarget, ScrollDamp, delta);

        if (Input.IsActionJustPressed(InputKeys.Editor.SelectModule)) {
            var mousePos = GetViewport().GetMousePosition();

            // Have to use a ray rather than MouseEntered/Exited as that doesn't work for
            // overlapping Area3Ds...
            //var space = 
            var camera = GetViewport().GetCamera3D();

            var query = new PhysicsRayQueryParameters3D();
            query.From = camera.ProjectRayOrigin(mousePos);
            query.To = query.From + camera.ProjectRayNormal(mousePos) * 100;
            query.CollideWithAreas = true;
            query.CollisionMask = Layers.Module.Body;

            var result = GetWorld3D().DirectSpaceState.ProjectRay(query);
            if (result != null) {
                var module = result.Collider.GetParent<Module>();
                if (module != null) {
                    EmitSignal(SignalName.ModuleSelected, module);
                }
            }
        }
    }
}
