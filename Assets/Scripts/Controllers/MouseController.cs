using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    public Camera Camera;
    public float ZDepth = 10;
    public float TorqueFactor = 0.1f;
    public float ThrustSpeed;

    public string BoostKey;
    public float BoostMultiplier;

    public string ThrustPositiveKey;
    public string ThrustNegativeKey;

    public string RollPositiveKey;
    public string RollNegativeKey;

    private Actor actor;
    private GameObject cursor;
    private float currentThrust;

    // Use this for initialization
    void Start () {
        actor = GetComponentInParent<Actor>();

        cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cursor.transform.localScale = Vector3.one * 0.2f;
	}

    void FixedUpdate() {
        // Heading
        Vector3 pos = Input.mousePosition;
        pos.z = ZDepth - Camera.transform.localPosition.z;
        pos = Camera.ScreenToWorldPoint(pos);
        //pos = Vector3.Slerp(transform.forward, pos, 0.9f);

        cursor.transform.position = pos;

        Vector3 delta = pos - transform.position;
        float angle = Vector3.Angle(transform.forward, delta);
        float p = Mathf.Clamp(angle / 45.0f, 0, 1);

        Vector3 cross = Vector3.Cross(transform.forward, delta);
        Vector3 torque = (cross * angle).normalized;

        torque *= actor.MaxTorque;
        torque *= MathsUtils.NormalDistribution(0.3f, 1, p);
        torque *= TorqueFactor;

        // Roll
        if (Input.GetButton(RollPositiveKey)) {
            torque += transform.forward * TorqueFactor;
        } else if (Input.GetButton(RollNegativeKey)) {
            torque -= transform.forward * TorqueFactor;
        }

        actor.AddTorque(torque);

        // Thrust
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

        float thrust = currentThrust;
        if (Input.GetButton(BoostKey)) {
            thrust *= BoostMultiplier;
        }

        if (thrust != 0) {
            Vector3 force = transform.forward * thrust;
            actor.AddForce(force);
        }
    }
}
