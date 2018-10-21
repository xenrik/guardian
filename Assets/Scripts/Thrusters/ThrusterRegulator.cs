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
    private bool acceptTorque;

    private float targetForce;
    private float currentForce;

    private void Start () {
        actor = GetComponentInParent<Actor>();
        actor.OnForceListener += OnForceApplied;
        actor.OnTorqueListener += OnTorqueApplied;

        ParticleSystem ps = GetComponent<ParticleSystem>();
        mainModule = ps.main;

        // Work out if we are a forward/reverse thruster, or just an attitude thruster
        if (transform.forward.Equals(-actor.gameObject.transform.forward)) {
            maxThrust = actor.MaxForwardThrust;
            acceptTorque = false;
        } else if (transform.forward.Equals(actor.gameObject.transform.forward)) {
            maxThrust = actor.MaxReverseThrust;
            acceptTorque = false;
        } else {
            maxThrust = actor.MaxTorque;
            acceptTorque = true;
            torqueVector = actor.GetComponent<Rigidbody>().CalculateTorqueForForceAtPosition(-transform.forward, transform.position, ForceMode.Impulse).normalized;
        }
	}

    private void OnDestroy() {
        if (actor) {
            actor.OnForceListener -= OnForceApplied;
            actor.OnTorqueListener -= OnTorqueApplied;
        }
    }

    private void FixedUpdate() {
        // Smooth it a little
        currentForce = Mathf.Lerp(currentForce, targetForce, 0.5f);
        float f = currentForce / maxThrust;

        mainModule.startSpeedMultiplier = f;
        mainModule.startSizeMultiplier = f;

        targetForce = 0;
    }

    private void OnForceApplied(object sender, Vector3 force) {
        float factor = CalculateEmissionFactor(-transform.forward, force);
        this.targetForce = (force.magnitude * factor);
    }

    private void OnTorqueApplied(object sender, Vector3 force) {
        if (!acceptTorque) {
            return;
        }

        torqueVector = actor.GetComponent<Rigidbody>().CalculateTorqueForForceAtPosition(-transform.forward, transform.position, ForceMode.Impulse).normalized;
        float factor = CalculateEmissionFactor(torqueVector, force);
        this.targetForce = (force.magnitude * factor);
    }

    private float CalculateEmissionFactor(Vector3 forward, Vector3 force) {
        // Find the angle between the target force and forward
        float angle = Vector3.Angle(forward, force);
        if (angle < 90) {
            // Get the percentile between zero and PI
            float d = Mathf.Clamp(1 - (angle / 90), 0, 1);

            // Normal distribution
            return MathsUtils.NormalDistribution(0.2f, 1, d) / 2.0f;
        }

        return 0;
    }
}
