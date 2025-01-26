using Godot;

public class SnapCollision {
    public readonly Module Module;
    public readonly string SnapName;
    public readonly Area3D OtherSnap;

    public readonly Area3D Snap;
    public readonly Module OtherModule;

    private string key;

    public SnapCollision(Module module, string snapName, Area3D otherSnap, Rid otherSnapRid) {
        Module = module;
        SnapName = snapName;
        OtherSnap = otherSnap;

        Snap = Module.IsQueuedForDeletion() ? null : Module.FindChild<Area3D>(SnapName);
        OtherModule = (otherSnap == null || otherSnap.IsQueuedForDeletion()) ? null : otherSnap.FindParent<Module>();

        key = module.GetInstanceId() + ":" + snapName + ":" + otherSnapRid.ToString();
    }

    public bool IsValid() {
        return Snap != null && OtherModule != null;
    }

    public override int GetHashCode() {
        return key.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (obj is SnapCollision other) {
            return other.key == key;
        }

        return false;
    }
}