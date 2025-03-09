using System;
using Godot;

public partial class MonitorArea : Area3D {
    private uint lastHash = 0;
    private uint processUpdates = 0;
    private uint physicsUpdates = 0;

    public override void _Process(double delta) {
        base._Process(delta);

        ++processUpdates;
        UpdateOverlappingAreas("Process");
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        ++physicsUpdates;
        UpdateOverlappingAreas("PhysicsProcess");
    }

    private void UpdateOverlappingAreas(string when) {
        var path = GetPath().ToString();
        var areas = GetOverlappingAreas();
        var message = path + " -> ";

        if (areas.IsEmpty()) {
            message += "None";
        } else {
            foreach (var area in areas) {
                message += area.GetPath().ToString() + " ";
            }
        }

        var hash = message.Hash();
        if (lastHash != hash) {
            Logger.Debug(when + $"{processUpdates},{physicsUpdates}:" + message);
            lastHash = hash;
            processUpdates = 0;
            physicsUpdates = 0;
        }
    }
}
