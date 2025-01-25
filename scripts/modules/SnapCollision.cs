using Godot;

public class SnapCollision {
    public readonly Module module;
    public readonly Area3D snap;

    public readonly Module otherModule;
    public readonly Area3D otherSnap;

    public SnapCollision() { }
    public SnapCollision(Module module, Area3D snap, Module otherModule, Area3D otherSnap) {
        this.module = module;
        this.snap = snap;

        this.otherModule = otherModule;
        this.otherSnap = otherSnap;
    }

    public override int GetHashCode() {
        return module.GetHashCode() ^ 7 ^
            snap.GetHashCode() ^ 7 ^
            otherModule.GetHashCode() ^ 7 ^
            otherSnap.GetHashCode();
    }

    public override bool Equals(object obj) {
        if (obj is SnapCollision other) {
            return other.module == module && other.snap == snap &&
                other.otherModule == otherModule && other.otherSnap == otherSnap;
        }

        return false;
    }
}