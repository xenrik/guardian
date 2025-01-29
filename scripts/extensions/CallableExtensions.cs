using System;
using Godot;

public static class CallableExtensions {
    public static void CallAfterPhysicsUpdate(this Callable callable, params Variant[] args) {
        SceneTree sceneTree = (SceneTree)Engine.GetMainLoop();
        Callable wrapper = Callable.From(() => {
            sceneTree.Connect(SceneTree.SignalName.PhysicsFrame,
                Callable.From(() => {
                    callable.Call(args);
                }),
                (uint)GodotObject.ConnectFlags.OneShot);
        });

        sceneTree.Connect(SceneTree.SignalName.PhysicsFrame,
            wrapper, (uint)GodotObject.ConnectFlags.OneShot);
    }

    public static void CallLater(this Callable callable, double delay, params Variant[] args) {
        SceneTree sceneTree = (SceneTree)Engine.GetMainLoop();
        var timer = sceneTree.CreateTimer(delay);
        timer.Timeout += () => {
            callable.Call(args);
        };
    }
}