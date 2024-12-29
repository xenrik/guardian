using Godot;

public partial class SimpleController : Node3D {
    [Export]
    public float Speed = 10;

    [Export]
    public float SprintSpeed = 100;

    public override void _Process(double delta) {
        base._Process(delta);

        Vector3 direction = Vector3.Zero;
        if (Input.IsKeyPressed(Key.W)) {
            direction += Vector3.Forward;
        }
        if (Input.IsKeyPressed(Key.S)) {
            direction -= Vector3.Forward;
        }
        if (Input.IsKeyPressed(Key.A)) {
            direction += Vector3.Right;
        }
        if (Input.IsKeyPressed(Key.D)) {
            direction -= Vector3.Right;
        }
        direction = direction.Normalized();
        if (Input.IsKeyPressed(Key.Shift)) {
            direction *= SprintSpeed;
        } else {
            direction *= Speed;
        }

        Position += direction * (float)delta;
    }
}