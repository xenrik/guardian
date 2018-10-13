using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    public Camera Camera;
    public float ZDepth = 10;
    public float TorqueFactor = 0.1f;

    private Actor actor;
    private GameObject cursor;

	// Use this for initialization
	void Start () {
        actor = GetComponentInParent<Actor>();

        cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cursor.transform.localScale = Vector3.one * 0.2f;
	}
	
	// Update is called once per frame
	void Update () {
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

        actor.AddTorque(torque);
	}
}
