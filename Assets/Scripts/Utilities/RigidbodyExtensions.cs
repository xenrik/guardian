using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RigidbodyExtensions {
    /**
     * Given a force and a position, calculate the torque it would impart
     * 
     * Shamelessly copied from:
     * https://forum.unity.com/threads/how-to-calculate-how-much-torque-will-rigidbody-addforceatposition-add.287164/#post-1901196
     */
    public static Vector3 CalculateTorqueForForceAtPosition(this Rigidbody rigidbody, Vector3 force, Vector3 position, ForceMode forceMode = ForceMode.Force) {
        Vector3 t = Vector3.Cross(position - rigidbody.worldCenterOfMass, force);
        t = ToDeltaTorque(rigidbody, t, forceMode);

        return t;
    }

    private static Vector3 ToDeltaTorque(Rigidbody rigidbody, Vector3 torque, ForceMode forceMode) {
        bool continuous = forceMode == ForceMode.Force || forceMode == ForceMode.Acceleration;
        bool useMass = forceMode == ForceMode.Force || forceMode == ForceMode.Impulse;

        if (continuous) {
            torque *= Time.fixedDeltaTime;
        }

        if (useMass) {
            torque = ApplyInertiaTensor(rigidbody, torque);
        }

        return torque;
    }

    private static Vector3 ApplyInertiaTensor(Rigidbody rigidbody, Vector3 v) {
        return rigidbody.rotation * Div(Quaternion.Inverse(rigidbody.rotation) * v, rigidbody.inertiaTensor);
    }

    private static Vector3 Div(Vector3 v, Vector3 v2) {
        return new Vector3(v.x / v2.x, v.y / v2.y, v.z / v2.z);
    }
}
