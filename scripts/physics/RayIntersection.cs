using Godot;
using Godot.Collections;

public class RayIntersection {
    public Node Collider {
        get {
            return (Node)entry["collider"];
        }
    }

    public string ColliderId {
        get {
            return (string)entry["collider_id"];
        }
    }

    public Vector3 Normal {
        get {
            return (Vector3)entry["normal"];
        }
    }

    public Vector3 Position {
        get {
            return (Vector3)entry["position"];
        }
    }

    public int FaceIndex {
        get {
            return (int)entry["face_index"];
        }
    }

    public Rid RID {
        get {
            return (Rid)entry["rid"];
        }
    }

    public int Shape {
        get {
            return (int)entry["shape"];
        }
    }

    private Dictionary entry;

    public RayIntersection(Dictionary entry) {
        this.entry = entry;
    }

    public override string ToString() {
        return $"RayIntersections[{Collider.Name}]";
    }
}