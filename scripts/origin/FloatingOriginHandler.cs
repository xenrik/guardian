using Godot;

public interface FloatingOriginHandler {
    /**
     * Called when a floating origin update has happened.
     * Return true if child nodes should also be called, or false if you handled that yourself
     */
    public bool OnFloatingOriginUpdated(Vector3 oldOrigin, Vector3 newOrigin);
}