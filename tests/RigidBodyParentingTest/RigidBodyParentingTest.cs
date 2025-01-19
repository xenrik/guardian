using Godot;
using System;

public partial class RigidBodyParentingTest : Node3D {
    [Export]
    public Vector3 Force = new Vector3(0, 0, 10);

    [Node]
    private RigidBody3D projectile;
    [Node]
    private RigidBody3D projectile2;

    [Node]
    private RigidBody3D cube;

    private bool reset = false;
    private Transform3D projectileHome;
    private Transform3D projectile2Home;
    private Transform3D cubeHome;

    public override void _Ready() {
        base._Ready();

        projectileHome = projectile.Transform;
        projectile2Home = projectile2.Transform;
        cubeHome = cube.Transform;
    }

    public override void _PhysicsProcess(double delta) {
        base._PhysicsProcess(delta);

        if (reset) {
            projectile.LinearVelocity = Vector3.Zero;
            projectile.AngularVelocity = Vector3.Zero;
            projectile.Transform = projectileHome;

            projectile2.LinearVelocity = Vector3.Zero;
            projectile2.AngularVelocity = Vector3.Zero;
            projectile2.Transform = projectile2Home;

            cube.LinearVelocity = Vector3.Zero;
            cube.AngularVelocity = Vector3.Zero;
            cube.Transform = cubeHome;

            reset = false;
        }
    }

    public void Shoot1() {
        var rotatedForce =
            projectile.Basis.X * Force.X +
            projectile.Basis.Y * Force.Y +
            projectile.Basis.Z * Force.Z
        ;
        projectile.ApplyCentralImpulse(rotatedForce);
    }

    public void Shoot2() {
        var rotatedForce =
            projectile2.Basis.X * Force.X +
            projectile2.Basis.Y * Force.Y +
            projectile2.Basis.Z * Force.Z
        ;
        projectile2.ApplyCentralImpulse(rotatedForce);
    }

    public void Reset() {
        reset = true;
    }

}
