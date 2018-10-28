using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleOscillator : MonoBehaviour {

    public float TotalDrift;
    public float MinSpeed;
    public float MaxSpeed;

    private Vector3 origin;

    private float direction = 1;
    private float offset;
    private float speed;

	// Use this for initialization
	void Start () {
        origin = transform.position;
        speed = Random.Range(MinSpeed, MaxSpeed);
	}
	
	// Update is called once per frame
	void Update () {
        offset += speed * direction * Time.deltaTime;
        if (direction == 1 && offset > TotalDrift) {
            offset = TotalDrift;
            direction = -1;
        } else if (direction == -1 && offset < 0) {
            offset = 0;
            direction = 1;
        }

        Vector3 position = origin;
        position.x += offset;
        transform.position = position;
	}
}
