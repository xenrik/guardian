using Godot;

public static class FloatingOriginExtensions {
    public static Vector3d GetUniversalPosition(this Node3D node) {
        FloatingOrigin origin = DependencyInjector.GetSingleton<FloatingOrigin>();
        return node.GlobalPosition + origin.Origin;
    }

    public static void SetUniversalPosition(this Node3D node, Vector3d position) {
        FloatingOrigin origin = DependencyInjector.GetSingleton<FloatingOrigin>();
        node.GlobalPosition = (Vector3)(position - origin.Origin);
    }
}