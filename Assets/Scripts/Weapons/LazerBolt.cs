using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LazerBolt : MonoBehaviour {

    public GameObject prefab;
    public GameObject target;
    public GameObject target2;

    public float Force;
    public float Delay;
    public float Lifetime;

    public float PhysicalDamage;
    public float ShieldDamage;

    public string FireKey;

    private float recharging;

    private void Start() {
        target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        target.transform.localScale = Vector3.one * 0.1f;
        target.GetComponent<MeshRenderer>().material.color = Color.red;

        target2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        target2.transform.localScale = Vector3.one * 0.1f;
        target2.GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    public void Update() {
        if (recharging > 0) {
            recharging -= Time.deltaTime;
        }

        if (Input.GetKey(FireKey) && recharging <= 0) {
            GameObject bolt = Instantiate(prefab);
            bolt.hideFlags = HideFlags.HideInHierarchy;

            bolt.transform.position = transform.position;
            bolt.transform.rotation = transform.rotation;

            StartCoroutine(AnimateBolt(bolt));
            recharging = Delay;
        }
    }

    public void FixedUpdate() {
        target.transform.position = ProjectPosition(1f);
    }

    /**
     * Project where the projectile will be after t seconds
     */
    public Vector3 ProjectPosition(float t) {
        //t += 1;

        // Work out where the target would be if we were not rotating
        Rigidbody myRb = prefab.GetComponent<Rigidbody>();
        float a = Force / myRb.mass;

        Vector3 p = transform.forward * (a/2) * t * t;
        target2.transform.position = transform.position + p;

        // Calculate how much we will have rotated
        GameObject ship = GetComponentInParent<Actor>().gameObject;
        Rigidbody shipRb = ship.GetComponent<Rigidbody>();

        Vector3 angularRotation = -shipRb.angularVelocity * t;
        Quaternion rotation = Quaternion.Euler(Mathf.Rad2Deg * angularRotation.x,
            Mathf.Rad2Deg * angularRotation.y,
            Mathf.Rad2Deg * angularRotation.z);

        // Adjust the final position
        p = rotation * p;
        return transform.position + p;
    }

    private IEnumerator AnimateBolt(GameObject bolt) {
        yield return new WaitForFixedUpdate();

        float ttl = Lifetime;
        Rigidbody rigidbody = bolt.GetComponent<Rigidbody>();

        //rigidbody.AddForce(bolt.transform.forward * Force, ForceMode.Impulse);
        while (ttl > 0) {
            rigidbody.AddForce(bolt.transform.forward * Force);

            yield return new WaitForFixedUpdate();
            ttl -= Time.fixedDeltaTime;
        }

        Destroy(bolt);
    }
}
