﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class LazerBolt : MonoBehaviour {

    public GameObject BoltPrefab;
    public GameObject TargetReticlePrefab;
    public GameObject HeadingReticle;

    public float BoltForce;
    public float BoltDelay;
    public float BoltLifetime;

    public float PhysicalDamage;
    public float ShieldDamage;

    public string FireKey;

    private Actor actor;
    private Rigidbody actorRidigbody;
    private float recharging;
    private float boltMass;

    private IEnumerable<Rigidbody> targets;
    private List<GameObject> reticles = new List<GameObject>();

    private Stack<GameObject> oldBolts = new Stack<GameObject>();

    private void Start() {
        boltMass = BoltPrefab.GetComponent<Rigidbody>().mass;

        actor = GetComponentInParent<Actor>();
        actorRidigbody = actor.GetComponent<Rigidbody>();

        float maxBullets = (BoltLifetime / BoltDelay) + 1;
        for (int i = 0; i < maxBullets; ++i) {
            oldBolts.Push(CreateBolt());
        }
    }

    public void Update() {
        if (recharging > 0) {
            recharging -= Time.deltaTime;
        }

        if (Input.GetKey(FireKey) && recharging <= 0) {
            StartCoroutine(FireBolt());
            recharging = BoltDelay;
        }

        targets = GameObject.FindGameObjectsWithTag("Target").Select(gameObject => gameObject.GetComponent<Rigidbody>());
        while (reticles.Count > targets.Count()) {
            GameObject reticle = reticles[reticles.Count - 1];
            reticles.RemoveAt(reticles.Count - 1);

            Destroy(reticle);
        }
        while (reticles.Count < targets.Count()) {
            reticles.Add(Instantiate(TargetReticlePrefab));
        }

        GameObject closestTarget = null;
        float closestDistance = 100f;
        Ray ray = new Ray(transform.position, transform.forward);
        foreach (Rigidbody target in targets) {
            Vector3 cross = Vector3.Cross(ray.direction, target.transform.position - ray.origin);
            if (cross.sqrMagnitude < closestDistance) {
                closestTarget = target.gameObject;
            }
        }

        if (closestTarget != null) {
            HeadingReticle.SetActive(true);
            HeadingReticle.transform.position = ray.origin + ray.direction * Vector3.Dot(ray.direction, closestTarget.transform.position - ray.origin);
        } else {
            HeadingReticle.SetActive(false);
        }
    }

    public void FixedUpdate() {
        if (targets != null) {
            int i = 0;
            foreach (Rigidbody target in targets) {
                GameObject reticle = reticles[i++];
                UpdateReticle(target, reticle);
            }
        }
    }

    private GameObject CreateBolt() {
        GameObject bolt = Instantiate(BoltPrefab);
        bolt.hideFlags = HideFlags.HideInHierarchy;
        bolt.SetActive(false);

        return bolt;
    }

    /**
     * Update the position of the reticle
     */
    private void UpdateReticle(Rigidbody target, GameObject reticle) {
        Vector3 targetVelocity = target.velocity;
        Vector3 relativePosition = target.transform.position - transform.position;
        float boltVelocity = (BoltForce / boltMass);

        float t = MathsUtils.CalculateInterceptTime(boltVelocity, relativePosition, targetVelocity);
        if (t <= 0) {
            reticle.transform.position = transform.position;
            reticle.transform.LookAt(actor.transform, actor.transform.up);

            //reticle.SetActive(false);
        } else {
            float reticleX = target.transform.position.x + (targetVelocity.x * t);
            float reticleY = target.transform.position.y + (targetVelocity.y * t);
            float reticleZ = target.transform.position.z + (targetVelocity.z * t);

            reticle.transform.position = new Vector3(reticleX, reticleY, reticleZ);
            reticle.transform.LookAt(actor.transform, actor.transform.up);
            //reticle.SetActive(true);
        }
    }

    private IEnumerator FireBolt() {
        yield return new WaitForFixedUpdate();

        GameObject bolt = null;
        if (oldBolts.Count == 0) {
            bolt = CreateBolt();
        } else {
            bolt = oldBolts.Pop();
        }

        bolt.SetActive(true);
        bolt.transform.position = transform.position;
        bolt.transform.rotation = transform.rotation;

        Rigidbody rigidbody = bolt.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.AddForce(bolt.transform.forward * BoltForce, ForceMode.Impulse);

        float ttl = BoltLifetime;
        while (ttl > 0) {
            yield return new WaitForFixedUpdate();
            ttl -= Time.fixedDeltaTime;
        }

        bolt.SetActive(false);
        oldBolts.Push(bolt);
    }
}
