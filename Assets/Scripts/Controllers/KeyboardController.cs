using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour {

    public string ThrustPositiveKey;
    public string ThrustNegativeKey;

    public string PitchPositiveKey;
    public string PitchNegativeKey;

    public string YawPositiveKey;
    public string YawNegativeKey;

    private Actor actor;

    void Start () {
        actor = GetComponent<Actor>();
	}
	
	void FixedUpdate () {
        Vector3 force = Vector3.zero;
        Vector3 torque = Vector3.zero;

        if (Input.GetButton(ThrustPositiveKey)) {
            force += transform.forward;
        } else if (Input.GetButton(ThrustNegativeKey)) {
            force -= transform.forward;
        }

        if (Input.GetButton(PitchPositiveKey)) {
            torque += transform.right;
        } else if (Input.GetButton(PitchNegativeKey)) {
            torque -= transform.right;
        }

        if (Input.GetButton(YawPositiveKey)) {
            torque -= transform.up;
        } else if (Input.GetButton(YawNegativeKey)) {
            torque += transform.up;
        }

        if (!force.Equals(Vector3.zero)) {
            force = force.normalized * actor.MaxThrust;
            actor.AddForce(force);
        }

        if (!torque.Equals(Vector3.zero)) {
            torque = torque.normalized * actor.MaxTorque;
            actor.AddTorque(torque);
        }
    }
}
