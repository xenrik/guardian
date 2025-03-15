using System;
using Godot;

public partial class DebugKeys : Node {
    public override void _Ready() {
        base._Ready();

        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed(InputKeys.Debug.PauseGame)) {
            GetTree().Paused = !GetTree().Paused;
        } else if (Input.IsActionJustPressed(InputKeys.Debug.ExitGame)) {
            GetTree().Quit();
        } else if (Input.IsActionJustPressed(InputKeys.Debug.CycleCameraPos)) {
            var cameraPositions = Groups.Debug.CameraPos.Members(GetTree());
            if (cameraPositions.IsEmpty()) {
                return;
            }

            var camera = GetViewport().GetCamera3D();
            var currentPos = cameraPositions.IndexOf(camera.GetParent());
            if (currentPos == -1) {
                return;
            }

            var nextPos = (currentPos + 1) % cameraPositions.Count;
            camera.Reparent(cameraPositions[nextPos], keepGlobalTransform: false);
        }
    }
}
