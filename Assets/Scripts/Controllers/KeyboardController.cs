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

    public float ThrustSpeed;

    private Actor actor;
    private float currentThrust;

    void Start () {
        actor = GetComponent<Actor>();
    }
	
	void FixedUpdate () {
        if (Input.GetButton(ThrustPositiveKey)) {
            if (currentThrust < 0) {
                currentThrust = 0;
            }
            float thrustDelta = actor.MaxForwardThrust * (Time.deltaTime / ThrustSpeed);
            currentThrust = Mathf.Min(actor.MaxForwardThrust, currentThrust + thrustDelta);
        } else if (Input.GetButton(ThrustNegativeKey)) {
            if (currentThrust > 0) {
                currentThrust = 0;
            }
            float thrustDelta = actor.MaxReverseThrust * (Time.deltaTime / ThrustSpeed);
            currentThrust = Mathf.Max(-actor.MaxReverseThrust, currentThrust - thrustDelta);
        } else {
            currentThrust = 0;
        }

        if (currentThrust != 0) {
            Vector3 force = transform.forward * currentThrust;
            actor.AddForce(force);
        }

        Vector3 torque = Vector3.zero;
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

        if (!torque.Equals(Vector3.zero)) {
            torque = torque.normalized * actor.MaxTorque;
            actor.AddTorque(torque);
        }
    }
}
