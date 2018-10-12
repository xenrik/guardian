using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour {
    public float MaxForwardThrust;
    public float MaxReverseThrust;

    public float MaxTorque;

    public event ForceListener OnForceListener;
    public event ForceListener OnTorqueListener;

    public delegate void ForceListener(object sender, Vector3 force);

    private new Rigidbody rigidbody;

    public void AddForce(Vector3 force) {
        rigidbody.AddForce(force);
        OnForceListener?.Invoke(this, force);
    }

    public void AddTorque(Vector3 torque) {
        rigidbody.AddTorque(torque);
        OnTorqueListener?.Invoke(this, torque);
    }

    private void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }
}
