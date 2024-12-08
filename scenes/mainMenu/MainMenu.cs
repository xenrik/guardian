using Godot;
using System;

public partial class MainMenu : Node3D {
    public void StartButtonPressed() {
        GD.Print("Start");
    }

    public void ShipEditorButtonPressed() {
        GD.Print("Ship Editor");
    }

    public void ExitButtonPressed() {
        GetTree().Quit();
    }
}
