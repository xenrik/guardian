using Godot;
using System;

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
		}
	}
}
