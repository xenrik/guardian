using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorSettings : MonoBehaviour {
    public float MaxThrust;
    public float MaxTorque;

    public Vector3 InstantaneousAcceleration {
        get; private set;
    }

    private new Rigidbody rigidbody;
    private Vector3 lastVelocity;

    private void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        Vector3 dv = rigidbody.velocity - lastVelocity;
        InstantaneousAcceleration = dv / Time.fixedDeltaTime;
    }
}
