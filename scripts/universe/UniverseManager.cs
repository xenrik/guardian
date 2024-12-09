using Godot;
using System;

[Singleton]
public partial class UniverseManager : Node {
    public Vector3 Origin { get; set; }

    private Node3D root;

    public void SetRoot(Node3D root, Vector3? origin = null) {
        this.root = root;
        Origin = origin != null ? (Vector3)origin : Vector3.Zero;
    }

    public void ClearRoot(Node3D root) {
        if (this.root == root) {
            this.root = null;
        }
    }
}
