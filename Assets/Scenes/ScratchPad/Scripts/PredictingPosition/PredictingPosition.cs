using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class PredictingPosition : MonoBehaviour {

    // The arrow that we are shooting from
    public GameObject Arrow;

    // The origin the bullets will have 
    public GameObject BulletOrigin;

    // The force imparted to a bullet when it is shot
    public float BulletForce = 10;

    // The delay between each bullet (s)
    public float BulletFrequency = 2;

    // The lifetime of each bullet (s)
    public float BulletLifetime = 1;

    // The mass of each bullet
    public float BulletMass = 1;

    // If true we will automatically pause the editor just before we destroy a bullet
    public bool PauseOnDestroy = false;

    // How long until we can shoot our next bullet
    private float nextBullet;

    // The rigidbody for the arrow
    private Rigidbody arrowRigidbody;

    // The location we predict the bullets will end up at (updated continuously based on angular velocity)
    private GameObject prediction; 

    // The location we calculated the bullets will end up at (calculated once when a bullet is shot)
    private GameObject targetPrediction;

    private void Start () {
        arrowRigidbody = Arrow.GetComponent<Rigidbody>();

        prediction = CreateSphere("Prediction", Color.red);
        targetPrediction = CreateSphere("TargetPrediction", Color.blue);
    }

    private void FixedUpdate() {
        UpdatePrediction();

        // Fire a bullet if we are ready to
        if (nextBullet <= 0) {
            StartCoroutine(FireBullet());
            nextBullet = BulletFrequency;
        } else {
            nextBullet -= Time.deltaTime;
        }
    }

    /**
     * Creates a simple sphere for debugging
     */
    private GameObject CreateSphere(string name, Color color) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.localScale = Vector3.one * 0.1f;
        sphere.GetComponent<MeshRenderer>().material.color = color;
        Destroy(sphere.GetComponent<SphereCollider>());

        return sphere;
    }

    /**
     * Update the position of the 'prediction' sphere
     */
    private void UpdatePrediction() {
        // Calculate where the bullet would be if we were not rotating
        float a = BulletForce / BulletMass;
        float v = a; // as we only impart a single impluse of force
        Vector3 predictedOffset = (Vector3.forward * v * BulletLifetime);

        // Calculate how much we are currently rotating
        Vector3 angularRotation = arrowRigidbody.angularVelocity * BulletLifetime;
        Quaternion predictedRotation = Quaternion.Inverse(Quaternion.Euler(
                Mathf.Rad2Deg * angularRotation.x,
                Mathf.Rad2Deg * angularRotation.y,
                Mathf.Rad2Deg * angularRotation.z));

        // Update the prediction
        prediction.transform.position = Arrow.transform.position
            + (Arrow.transform.rotation * predictedRotation * predictedOffset)
            + (Arrow.transform.rotation * predictedRotation * BulletOrigin.transform.localPosition);
    }

    /**
     * Coroutine -- Create a fire a bullet. Also positions the 'target' sphere
     * where we expect the bullet to end up at when we destroy it
     */
    private IEnumerator FireBullet() {
        // Prepare the new bullet
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Bullet";
        bullet.transform.position = BulletOrigin.transform.position;
        bullet.transform.rotation = BulletOrigin.transform.rotation;
        bullet.transform.localScale = Vector3.one * 0.2f;

        Rigidbody bulletRigidbody = bullet.AddComponent<Rigidbody>();
        bulletRigidbody.mass = BulletMass;
        bulletRigidbody.useGravity = false;
        bulletRigidbody.drag = 0;

        // Ensure we're in a FixedUpdate and then impart the force
        yield return new WaitForFixedUpdate();
        bulletRigidbody.AddForce(bullet.transform.forward * BulletForce, ForceMode.Impulse);

        // Show position this bullet should end up at
        float a = BulletForce / BulletMass;
        float v = a; // as we only impart a single impluse of force
        Vector3 predictedOffset = bullet.transform.forward * v * BulletLifetime;
        targetPrediction.transform.position = bullet.transform.position + predictedOffset;

        // Wait for the bullet to timeout
        float ttl = BulletLifetime;
        while (ttl > 0) {
            yield return new WaitForFixedUpdate();
            ttl -= Time.fixedDeltaTime;
        }

        if (PauseOnDestroy) {
            PauseOnDestroy = false;
            Debug.Break();

            // Yield otherwise the bullet will get destroyed before the editor pauses
            yield return new WaitForFixedUpdate();
        }

        Destroy(bullet);
    }
}
