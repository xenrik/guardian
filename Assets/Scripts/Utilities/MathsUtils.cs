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
     * 
     * If sigma=0.2f, mu=1, then the result will be normally distributed with a peak of 1, for the value 1.
     */
    public static float NormalDistribution(float sigma, float mu, float value) {
        float sigma_sq = sigma * sigma;
        float value_mu_sq = (value - mu) * (value - mu);

        return 1 / (Mathf.Sqrt(2 * Mathf.PI * sigma_sq)) * Mathf.Exp(-value_mu_sq / (2 * sigma_sq));
    }

    /**
     * Given a targets position and velocity (relative to some origin), calculate how long
     * it will take to intercept at a given speed. Returns zero if no interception is possible at the
     * given speed
     */
    public static float CalculateInterceptTime(float speed, Vector3 targetPosition, Vector3 targetVelocity) {
        float vSqr = targetVelocity.sqrMagnitude;
        if (vSqr < 0.001) {
            // It's effectively not moving relative to us -- shouldn't this be speed??
            return 0;
        }

        float a = vSqr - (speed * speed);

        // Handle similar velocities
        if (Mathf.Abs(a) < 0.001) {
            float t = -targetPosition.sqrMagnitude / (2 * Vector3.Dot(targetVelocity, targetPosition));

            // Don't shoot back in time
            return Mathf.Max(t, 0);
        }

        float b = 2 * Vector3.Dot(targetVelocity, targetPosition);
        float c = targetPosition.sqrMagnitude;
        float determinant = b * b - 4 * a * c;

        if (determinant > 0) {
            // Two intercept paths
            float dSqrt = Mathf.Sqrt(determinant);
            float t1 = (-b + dSqrt) / (2 * a);
            float t2 = (-b - dSqrt) / (2 * a);

            if (t1 > 0) {
                if (t2 > 0) {
                    // Both are position, return the smallest
                    return Mathf.Min(t1, t2);
                } else {
                    // Only t1 is positive
                    return t1;
                }
            } else {
                // Either t2 is positive, or we can't shoot back in time?
                return Mathf.Max(t2, 0);
            }
        } else if (determinant < 0) {
            // No interception possible
            return 0;
        } else {
            // Only one intercept path (or none)
            return Mathf.Max(-b / (2 * a), 0);
        }
    }
}
