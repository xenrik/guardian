using System;
using Godot;

public static class CallableExtensions {
    private static void CallAfterCount(string signalName, int delay, Callable callable, Variant[] args) {
        SceneTree sceneTree = (SceneTree)Engine.GetMainLoop();
        int counter = delay;

        Callable wrapper = default;
        Action action = () => {
            if (counter-- == 0) {
                callable.Call(args);
            } else {
                sceneTree.Connect(signalName, wrapper, (uint)GodotObject.ConnectFlags.OneShot);
            }
        };

        wrapper = Callable.From(action);
        sceneTree.Connect(signalName, wrapper, (uint)GodotObject.ConnectFlags.OneShot);
    }

    public static void CallAfterFrame(this Callable callable, int frameCount = 1, params Variant[] args) {
        CallAfterCount(SceneTree.SignalName.ProcessFrame, frameCount, callable, args);
    }

    public static void CallAfterPhysicsFrame(this Callable callable, int frameCount = 1, params Variant[] args) {
        CallAfterCount(SceneTree.SignalName.PhysicsFrame, frameCount, callable, args);
    }

    public static void CallAfterDelay(this Callable callable, double delayS, params Variant[] args) {
        SceneTree sceneTree = (SceneTree)Engine.GetMainLoop();
        var timer = sceneTree.CreateTimer(delayS);
        timer.Timeout += () => {
            callable.Call(args);
        };
    }
}