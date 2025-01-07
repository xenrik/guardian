using Godot;
using System;

public partial class EditorModule : Node3D {
    [Signal]
    public delegate void MouseEnteredEventHandler(EditorModule module);
    [Signal]
    public delegate void MouseExitedEventHandler(EditorModule module);
    [Signal]
    public delegate void SnapCollisionEnteredEventHandler();
    [Signal]
    public delegate void SnapCollisionExitedEventHandler();
    [Signal]
    public delegate void BodyCollisionEnteredEventHandler();
    [Signal]
    public delegate void BodyCollisionExitedEventHandler();

    public void OnMouseEnter() {
        EmitSignal(SignalName.MouseEntered, this);
    }

    public void OnMouseExit() {
        EmitSignal(SignalName.MouseExited, this);
    }

    public void OnSnapCollisionEnter(Rid bodyRid, Node body, int bodyShapeIndex, int localShapeIndex) {
        //EmitSignal(SignalName.SnapCollisionEntered, this, collision);
    }

    public void OnSnapCollisionExit(Rid bodyRid, Node body, int bodyShapeIndex, int localShapeIndex) {
        // EmitSignal(SignalName.SnapCollisionExited, this, collision);
    }

    public void OnBodyCollisionEnter() {
        EmitSignal(SignalName.BodyCollisionEntered, this);
    }

    public void OnBodyCollisionExit() {
        EmitSignal(SignalName.BodyCollisionExited, this);
    }
}
