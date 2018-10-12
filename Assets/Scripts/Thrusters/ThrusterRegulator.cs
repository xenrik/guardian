using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ThrusterRegulator : MonoBehaviour {

    public bool DebugEnabled;

    private Actor actor;

    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;

    private float maxThrust;
    private Vector3 torqueVector;

    private float targetForce;
    private float currentForce;

    private void Start () {
        actor = GetComponentInParent<Actor>();
        actor.OnForceListener += OnForceApplied;
        actor.OnTorqueListener += OnTorqueApplied;

        ParticleSystem ps = GetComponent<ParticleSystem>();
        mainModule = ps.main;

        // Work out if we are a forward/reverse thruster, or just an attitude thruster
        if (transform.forward.Equals(actor.gameObject.transform.forward)) {
            maxThrust = actor.MaxForwardThrust;
        } else if (transform.forward.Equals(-actor.gameObject.transform.forward)) {
            maxThrust = actor.MaxReverseThrust;
        } else {
            maxThrust = actor.MaxTorque;
            torqueVector = actor.GetComponent<Rigidbody>().CalculateTorqueForForceAtPosition(-transform.forward, transform.position, ForceMode.Impulse).normalized;
        }
	}

    private void OnDestroy() {
        if (actor) {
            actor.OnForceListener -= OnForceApplied;
            actor.OnTorqueListener -= OnTorqueApplied;
        }
    }

    private void Update () {
        currentForce = Mathf.Lerp(currentForce, targetForce, 0.5f);
        float f = currentForce / maxThrust;
        mainModule.startSpeedMultiplier = f;
        mainModule.startSizeMultiplier = f;
        targetForce = 0;
	}

    private void OnForceApplied(object sender, Vector3 force) {
        float factor = CalculateEmissionFactor(-transform.forward, force);
        this.targetForce = maxThrust * factor;
    }

    private void OnTorqueApplied(object sender, Vector3 force) {
        float factor = CalculateEmissionFactor(torqueVector, force);
        this.targetForce = maxThrust * factor;
    }

    private float CalculateEmissionFactor(Vector3 forward, Vector3 v) {
        // Find the angle between the target force and forward
        float costheta = Vector3.Dot(forward, v) / (forward.magnitude * v.magnitude);
        float theta = Mathf.Acos(costheta);
        if (theta > Mathf.PI) {
            theta -= Mathf.PI;
        }

        if (theta > -Mathf.PI && theta < Mathf.PI) {
            // Get the percentile between zero and PI
            float d = Mathf.Clamp(1 - (Mathf.Abs(theta) / Mathf.PI), 0, 1);

            // Normal distribution
            return MathsUtils.NormalDistribution(0.2f, 1, d) / 2.0f;
        }

        return 0;
    }
}
