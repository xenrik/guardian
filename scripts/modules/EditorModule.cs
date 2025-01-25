using Godot;
using Godot.Collections;

public partial class EditorModule : Module {
    [Signal]
    public delegate void MouseEnteredEventHandler(EditorModule module);
    [Signal]
    public delegate void MouseExitedEventHandler(EditorModule module);

    public void OnMouseEnter() {
        EmitSignal(SignalName.MouseEntered, this);
    }

    public void OnMouseExit() {
        EmitSignal(SignalName.MouseExited, this);
    }
}
