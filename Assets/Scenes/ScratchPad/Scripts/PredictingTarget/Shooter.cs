﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class Shooter : MonoBehaviour {

    public Text Text;
    public GameObject BulletOrigin;
    public GameObject Target;
    public PIDController headingController;

    public float MaxTorque;

    public float BulletInterval;
    public float BulletForce;
    public float BulletMass;
    public float BulletLifetime;

    private Rigidbody shooterRigidbody;
    private Rigidbody targetRigidbody;

    private GameObject singlePrediction;
    private GameObject constantPrediction;

    private float nextBullet;
    private string[] textLines = new string[3];

	// Use this for initialization
	void Start () {
        singlePrediction = CreateSphere("SinglePrediction", Color.blue, 1);
        constantPrediction = CreateSphere("ConstantPrediction", Color.red, 1);

        shooterRigidbody = GetComponent<Rigidbody>();
        targetRigidbody = Target.GetComponent<Rigidbody>();
    }

    private void Update() {
        Text.text = string.Join("\n", textLines);
    }

    // Update is called once per frame
    void FixedUpdate () {
        float t = UpdatePrediction(constantPrediction);
        Vector3 target = constantPrediction.transform.position;
        Vector3 requiredHeading = Quaternion.LookRotation((target - transform.position).normalized, transform.up).eulerAngles;        
        Vector3 currentHeading = transform.rotation.eulerAngles;

        if (Mathf.Abs(requiredHeading.x - 360 - currentHeading.x) < Mathf.Abs(requiredHeading.x - currentHeading.x)) {
            requiredHeading.x -= 360;
        } else if (Mathf.Abs(requiredHeading.x + 360 - currentHeading.x) < Mathf.Abs(requiredHeading.x - currentHeading.x)) {
            requiredHeading.x += 360;
        }
        if (Mathf.Abs(requiredHeading.y - 360 - currentHeading.y) < Mathf.Abs(requiredHeading.y - currentHeading.y)) {
            requiredHeading.y -= 360;
        } else if (Mathf.Abs(requiredHeading.y + 360 - currentHeading.y) < Mathf.Abs(requiredHeading.y - currentHeading.y)) {
            requiredHeading.y += 360;
        }
        if (Mathf.Abs(requiredHeading.z - 360 - currentHeading.z) < Mathf.Abs(requiredHeading.z - currentHeading.z)) {
            requiredHeading.z -= 360;
        } else if (Mathf.Abs(requiredHeading.z + 360 - currentHeading.z) < Mathf.Abs(requiredHeading.z - currentHeading.z)) {
            requiredHeading.z += 360;
        }

        Vector3 delta = requiredHeading - currentHeading;
        Vector3 torque = headingController.Update(delta, Time.fixedDeltaTime);
        torque = Vector3.ClampMagnitude(torque, MaxTorque);

        textLines[1] = $"Required: {requiredHeading} - Current: {currentHeading}\nDelta: {delta}-{delta.magnitude} - Torque: {torque}-{torque.magnitude}";
        shooterRigidbody.AddTorque(torque);

        //transform.rotation = Quaternion.Euler(requiredHeading);

        if (nextBullet <= 0) {
            if (t < BulletLifetime) {
                singlePrediction.transform.position = constantPrediction.transform.position;
                singlePrediction.SetActive(true);

                StartCoroutine(Shoot());
                nextBullet = BulletInterval;
            } else {
                singlePrediction.SetActive(false);
            }
        } else {
            nextBullet -= Time.fixedDeltaTime;
        }
    }

    private float UpdatePrediction(GameObject prediction) {
        Vector3 targetVelocity = targetRigidbody.velocity;
        Vector3 targetPosition = Target.transform.position;
        float bulletVelocity = (BulletForce / BulletMass);

        float t = CalculateInterceptTime(bulletVelocity, targetPosition, targetVelocity);
        if (t <= 0) { 
            prediction.transform.position = BulletOrigin.transform.position;
        } else {
            float aimX = Target.transform.position.x + (targetVelocity.x * t);
            float aimY = Target.transform.position.y + (targetVelocity.y * t);
            float aimZ = Target.transform.position.z + (targetVelocity.z * t);

            prediction.transform.position = new Vector3(aimX, aimY, aimZ);
        }

        return t;
    }

    private IEnumerator Shoot() {
        float d = (BulletForce / BulletMass) * BulletLifetime;
        textLines[0] = $"Bullet Range: {d}m";

        GameObject bullet = CreateSphere("Bullet", Color.white);
        bullet.transform.position = BulletOrigin.transform.position;
        bullet.transform.rotation = BulletOrigin.transform.rotation;

        Rigidbody rigidbody = bullet.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.mass = BulletMass;
        rigidbody.drag = 0;

        rigidbody.AddForce(transform.forward * BulletForce, ForceMode.Impulse);
        float ttl = BulletLifetime;
        while (ttl > 0) {
            yield return new WaitForFixedUpdate();
            ttl -= Time.fixedDeltaTime;
        }

        Destroy(bullet);
    }

    /**
     * Given a targets position and velocity (relative to some origin), calculate how long
     * it will take to intercept at a given speed. Returns zero if no interception is possible at the
     * given speed
     */
    public static float CalculateInterceptTime(float speed, Vector3 targetPosition, Vector3 targetVelocity) {
        float vSqr = targetVelocity.sqrMagnitude;
        if (vSqr < 0.001) {
            // It's effectively not moving relative to us -- shouldn't this be speed??
            return 0;
        }

        float a = vSqr - (speed * speed);

        // Handle similar velocities
        if (Mathf.Abs(a) < 0.001) {
            float t = -targetPosition.sqrMagnitude / (2 * Vector3.Dot(targetVelocity, targetPosition));

            // Don't shoot back in time
            return Mathf.Max(t, 0);
        }

        float b = 2 * Vector3.Dot(targetVelocity, targetPosition);
        float c = targetPosition.sqrMagnitude;
        float determinant = b * b - 4 * a * c;

        if (determinant > 0) {
            // Two intercept paths
            float dSqrt = Mathf.Sqrt(determinant);
            float t1 = (-b + dSqrt) / (2 * a);
            float t2 = (-b - dSqrt) / (2 * a);

            if (t1 > 0) {
                if (t2 > 0) {
                    // Both are position, return the smallest
                    return Mathf.Min(t1, t2);
                } else {
                    // Only t1 is positive
                    return t1;
                }
            } else {
                // Either t2 is positive, or we can't shoot back in time?
                return Mathf.Max(t2, 0);
            }
        } else if (determinant < 0) {
            // No interception possible
            return 0;
        } else {
            // Only one intercept path (or none)
            return Mathf.Max(-b / (2 * a), 0);
        }
    }

    /**
     * Creates a simple sphere for debugging
     */
    private GameObject CreateSphere(string name, Color color, float size = 0.1f) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.localScale = Vector3.one * size;
        sphere.GetComponent<MeshRenderer>().material.color = color;
        Destroy(sphere.GetComponent<SphereCollider>());

        return sphere;
    }
}
