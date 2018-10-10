using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathsUtils {
    /**
     * Given a rigid body, calculate the maximum velocity it could reach given
     * a constant force and infinite time. If there is no drag, this will be infinity!
     */
    public static float GetMaxVelocity(Rigidbody rigidbody, float force) {
        return ((force / rigidbody.drag) - Time.fixedDeltaTime * force) / rigidbody.mass;
    }

    /**
     * Given a rigid body, calculate the final velocity it could reach given
     * a constant force over a given time. If drag is non-zero, this has to use an iterative
     * approach to calculate the velocity, one calculation for each second.
     */
    public static float GetFinalVelocity(Rigidbody rigidbody, float force, float time) {
        float a = (force / rigidbody.mass);
        float v = 0;
        if (rigidbody.drag > 0) {
            for (float dt = time; dt >= 0; dt -= 1.0f) {
                float dragForce = Mathf.Pow(v, 2) * rigidbody.drag;
                v += Mathf.Max(0, a - dragForce);
            }
        } else {
            v = a * time;
        }

        return v;
    }
}
