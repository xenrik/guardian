using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ThrusterRegulator : MonoBehaviour {

    public Text text;

    private ActorSettings actorSettings;

    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;

	// Use this for initialization
	void Start () {
        actorSettings = GetComponentInParent<ActorSettings>();

        ParticleSystem ps = GetComponent<ParticleSystem>();
        mainModule = ps.main;
	}
	
	// Update is called once per frame
	void Update () {

        if (text) {
            text.text = $"{actorSettings.InstantaneousAcceleration} - {actorSettings.InstantaneousAcceleration.magnitude}";
        }

        mainModule.startLifetimeMultiplier = actorSettings.InstantaneousAcceleration.magnitude / actorSettings.MaxThrust;
	}
}
