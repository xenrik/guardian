using Godot;
using System;

public partial class SimpleOscillator : RigidBody3D {
    [Export]
    public Vector3 Range;

    [Export]
    public float Speed = 1;

    [Export]
    public PidSettings PID;

    private PidController<Vector3> controller;

    private Vector3 origin;
    private float offset;

    public override void _Ready() {
        base._Ready();

        controller = PidController<Vector3>.Instantiate(Position, PID);
        origin = Position;
    }

    public override void _PhysicsProcess(double delta) {
        offset += Speed * (float)delta;

        float s = Mathf.Sin(offset);

        Vector3 target = origin + (Range * s);
        controller.Target = target;
        controller.Current = Position;

        //GD.Print("Origin: " + origin + " - Target: " + controller.Target + " (" + target + " - " + s + ") - Current: " + Position);

        Vector3 impulse = controller.Update((float)delta);
        //GD.Print("Impulse: " + impulse);
        ApplyImpulse(impulse);
    }
}
