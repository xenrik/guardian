using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ThrusterRegulator : MonoBehaviour {

    private Actor actor;

    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;

    private float force;
    private float currentForce;

	// Use this for initialization
	void Start () {
        actor = GetComponentInParent<Actor>();
        actor.OnForceListener += OnForceApplied;

        ParticleSystem ps = GetComponent<ParticleSystem>();
        mainModule = ps.main;
	}

	// Update is called once per frame
	void Update () {
        currentForce = Mathf.Lerp(currentForce, force, 0.5f);
        float f = currentForce / actor.MaxThrust;
        mainModule.startSpeedMultiplier = f;
        mainModule.startSizeMultiplier = f;
        force = 0;
	}

    private void OnForceApplied(object sender, Vector3 force) {
        this.force += force.magnitude / Time.fixedDeltaTime;
        this.force = Mathf.Min(this.force, actor.MaxThrust);
    }
}
