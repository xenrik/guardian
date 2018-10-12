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

    /**
     * Get a normal distribution of the given value with a given sigma and mu
     */
    public static float NormalDistribution(float sigma, float mu, float value) {
        float sigma_sq = sigma * sigma;
        float value_mu_sq = (value - mu) * (value - mu);

        return 1 / (Mathf.Sqrt(2 * Mathf.PI * sigma_sq)) * Mathf.Exp(-value_mu_sq / (2 * sigma_sq));
    }
}
