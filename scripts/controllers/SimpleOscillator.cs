using Godot;
using System;

public partial class SimpleOscillator : RigidBody2D {
    [Export]
    public Vector2 Range;

    [Export]
    public float Speed = 1;

    [Export]
    public PidSettings PID;

    private PidController<Vector2> controller;

    private Vector2 origin;
    private float offset;

    public override void _Ready() {
        base._Ready();

        controller = PidController<Vector2>.Instantiate(Position, PID);
        origin = Position;
    }

    public override void _PhysicsProcess(double delta) {
        offset += Speed * (float)delta;

        float s = Mathf.Sin(offset);

        Vector2 target = origin + (Range * s);
        controller.Target = target;
        controller.Current = Position;

        //GD.Print("Origin: " + origin + " - Target: " + controller.Target + " (" + target + " - " + s + ") - Current: " + Position);

        Vector2 impulse = controller.Update((float)delta);
        //GD.Print("Impulse: " + impulse);
        ApplyImpulse(impulse);
    }
}
