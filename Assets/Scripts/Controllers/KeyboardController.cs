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

    private ActorSettings actorSettings;
    private new Rigidbody rigidbody;

    void Start () {
        rigidbody = GetComponent<Rigidbody>();
        actorSettings = GetComponent<ActorSettings>();
	}
	
	void FixedUpdate () {
        if (Input.GetButton(ThrustPositiveKey)) {
            rigidbody.AddForce(transform.forward * actorSettings.MaxThrust);
        } else if (Input.GetButton(ThrustNegativeKey)) {
            rigidbody.AddForce(transform.forward * -actorSettings.MaxThrust);
        }

        if (Input.GetButton(PitchPositiveKey)) {
            rigidbody.AddTorque(transform.right * actorSettings.MaxTorque);
        } else if (Input.GetButton(PitchNegativeKey)) {
            rigidbody.AddTorque(transform.right * -actorSettings.MaxTorque);
        }

        if (Input.GetButton(YawPositiveKey)) {
            rigidbody.AddTorque(transform.up * -actorSettings.MaxTorque);
        } else if (Input.GetButton(YawNegativeKey)) {
            rigidbody.AddTorque(transform.up * actorSettings.MaxTorque);
        }
    }
}
