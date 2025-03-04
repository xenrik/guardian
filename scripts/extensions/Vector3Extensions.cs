using Godot;

public static class Vector3Extensions {
    /**
     * A SmoothDamp function moving the current vector towards another smoothly
     */
    public static Vector3 Damp(this Vector3 vec, Vector3 target, float lambda, double dt) {
        return vec.Lerp(target, (float)(1 - Mathf.Exp(-lambda * dt)));
    }
}