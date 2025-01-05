using Godot;
using System;

public partial class PhysicsNoodles : Node2D {
    [Export]
    Vector2 Size = new Vector2(100, 100);

    [Export]
    Vector2 Frequency = new Vector2(10, 10);

    [Export]
    float NoodleSize = 1;

    [Export]
    PidSettings ResetPID;

    [Export]
    float ResetScale = 0.5f;

    [Export]
    uint CollisionLayer = uint.MaxValue;

    [Export]
    uint CollisionMask = uint.MaxValue;

    [Export]
    int test = 0;

    private Noodle noodle;

    public override void _Ready() {
        base._Ready();

        float offsetX = -Size.X / 2.0f;
        float offsetY = -Size.Y / 2.0f;

        for (float y = 0; y < Size.Y; y += Frequency.Y) {
            for (float x = 0; x < Size.X; x += Frequency.X) {
                Noodle noodle = new Noodle();
                noodle.NoodleSize = NoodleSize;
                noodle.Position = new Vector2(x + offsetX, y + offsetY);
                noodle.Controller = PidController<Vector2>.Instantiate(noodle.Position, ResetPID);
                noodle.ResetScale = ResetScale;
                noodle.CollisionLayer = CollisionLayer;
                noodle.CollisionMask = CollisionMask;

                this.noodle = noodle;

                AddChild(noodle);

                if (test > 0) {
                    break;
                }
            }

            if (test > 0) {
                break;
            }
        }
    }

    public override void _Process(double delta) {
        base._Process(delta);

        if (test > 1) {
            noodle.ApplyImpulse(Vector2.Up * 0.1f);
            test = 1;
        }
    }

    public override void _Draw() {
        float offsetX = -Size.X / 2.0f;
        float offsetY = -Size.Y / 2.0f;

        for (float y = 0; y < Size.Y; y += Frequency.Y) {
            for (float x = 0; x < Size.X; x += Frequency.X) {
                DrawCircle(new Vector2(x + offsetX, y + offsetY), NoodleSize, ColorConstants.RED);
            }
        }
    }

    private partial class Noodle : RigidBody2D {
        private Vector2 origin;
        public PidController<Vector2> Controller;
        public float NoodleSize;
        public float ResetScale;

        public override void _Ready() {
            base._Ready();

            origin = Position;
            GravityScale = 0;
            Mass = 0.001f;

            CircleShape2D shape = new CircleShape2D();
            shape.Radius = NoodleSize;

            CollisionShape2D collision = new CollisionShape2D();
            collision.Shape = shape;

            AddChild(collision);
        }

        public override void _PhysicsProcess(double delta) {
            base._PhysicsProcess(delta);

            Controller.Target = origin;
            Controller.Current = Position;

            Vector2 force = Controller.Update((float)delta) * ResetScale;
            if (force.LengthSquared() > Mathf.Epsilon) {
                ApplyForce(force);
                QueueRedraw();
            }
        }

        public override void _Draw() {
            DrawCircle(Vector2.Zero, NoodleSize, ColorConstants.WHITE);
        }
    }
}
