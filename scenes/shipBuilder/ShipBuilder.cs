using Godot;
using System;

public partial class ShipBuilder : Node3D {
    private EditorModule currentModule = null;

    private bool isEditing = false;

    private Vector3 moduleDragStart;
    private Vector3 dragStart;

    public override void _Process(double delta) {
        base._Process(delta);

        if (Input.IsActionPressed(InputKeys.Editor.SelectModule) && currentModule != null) {
            var camera = GetViewport().GetCamera3D();
            var currentPos = camera.ProjectPosition(GetViewport().GetMousePosition(), camera.GlobalPosition.Y);

            if (isEditing) {
                // Drag
                var dragDelta = currentPos - dragStart;
                var newPos = moduleDragStart + dragDelta;
                newPos.Y = moduleDragStart.Y;

                currentModule.GlobalPosition = newPos;
            } else {
                // Start editing
                isEditing = true;

                moduleDragStart = currentModule.GlobalPosition;
                dragStart = currentPos;
            }
        } else if (isEditing && currentModule != null) {
            // Stop editing
            isEditing = false;
        }
    }

    private void OnModuleEntered(EditorModule module) {
        if (isEditing) {
            return;
        }

        currentModule = module;
    }

    private void OnModuleExited(EditorModule module) {
        if (isEditing) {
            return;
        }

        if (currentModule == module) {
            currentModule = null;
        }
    }
}
