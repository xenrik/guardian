using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMover : MonoBehaviour {

    public float ThrustForce;

    public Vector3 TorqueForce;
    public float TorqueInterval;

    private new Rigidbody rigidbody;
    private float torqueImpulse;

	private void Start () {
        rigidbody = GetComponent<Rigidbody>();
        torqueImpulse = TorqueInterval;
	}
	
	private void FixedUpdate () {
        rigidbody.AddForce(transform.forward * ThrustForce);
        if (torqueImpulse <= 0) {
            rigidbody.AddTorque(TorqueForce, ForceMode.Impulse);
            torqueImpulse = TorqueInterval;
        } else {
            torqueImpulse -= Time.fixedDeltaTime;
        }
	}
}
