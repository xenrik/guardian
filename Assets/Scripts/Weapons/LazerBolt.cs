using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LazerBolt : MonoBehaviour {

    public GameObject prefab;

    public float Force;
    public float Delay;
    public float Lifetime;

    public float PhysicalDamage;
    public float ShieldDamage;

    public string FireKey;

    private float recharging;

    public void Update() {
        if (recharging > 0) {
            recharging -= Time.deltaTime;
        }

        if (Input.GetKey(FireKey) && recharging <= 0) {
            GameObject bolt = Instantiate(prefab);
            bolt.hideFlags = HideFlags.HideAndDontSave;

            bolt.transform.position = transform.position;
            bolt.transform.rotation = transform.rotation;

            StartCoroutine(AnimateBolt(bolt));
            recharging = Delay;
        }
    }

    private IEnumerator AnimateBolt(GameObject bolt) {
        float ttl = Lifetime;
        Rigidbody rigidbody = bolt.GetComponent<Rigidbody>();

        rigidbody.AddForce(bolt.transform.forward * Force, ForceMode.Impulse);
        while (ttl > 0) {
            rigidbody.AddForce(bolt.transform.forward * Force);
            yield return new WaitForFixedUpdate();

            ttl -= Time.fixedDeltaTime;
        }

        Destroy(bolt);
    }
}
