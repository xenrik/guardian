using Godot;
using System;

public partial class ShipBuilder : Node3D {
    private EditorModule currentModule = null;
    private bool isEditing = false;

    public override void _Process(double delta) {
        base._Process(delta);

        if (Input.IsActionPressed(InputKeys.Editor.SelectModule) && currentModule != null) {
            if (isEditing) {
                // Drag
            } else {
                // Start editing
                isEditing = true;
            }
        } else if (isEditing && currentModule != null) {
            // Stop editing
            isEditing = false;
        }
    }

    private void OnModuleEntered(EditorModule module) {
        currentModule = module;
    }

    private void OnModuleExited(EditorModule module) {
        if (currentModule == module) {
            currentModule = null;
        }
    }
}
