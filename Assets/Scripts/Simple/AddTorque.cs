using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTorque : MonoBehaviour {

    public Vector3 Force;
    public ForceMode ForceMode = ForceMode.Force;

    private new Rigidbody rigidbody;
    private float interval;

	// Use this for initialization
	void Start () {
        rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        rigidbody.AddTorque(Force, ForceMode);    	

        if (ForceMode == ForceMode.Impulse) {
            this.enabled = false;
        }
	}
}
