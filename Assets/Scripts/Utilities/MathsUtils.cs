using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathsUtils {
    /**
     * Given a rigid body, calculate the maximum velocity it could reach given
     * a constant force
     */
    public static float GetMaxVelocity(Rigidbody rigidbody, float force) {
        return ((force / rigidbody.drag) - Time.fixedDeltaTime * force) / rigidbody.mass;
    }
}
