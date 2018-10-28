using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindClosest : MonoBehaviour {

    public UnityEngine.UI.Text text;

    public float UseOriginDistanceRange = 1;
    public float MaxDistanceToSelect = 10;

    private GameObject[] spheres;
    private Outline[] outlines;
    private float maxDistance;

	// Use this for initialization
	void Start () {
        spheres = new GameObject[transform.childCount];
        maxDistance = MaxDistanceToSelect * MaxDistanceToSelect;

        outlines = new Outline[spheres.Length];
        for (int i = 0; i < spheres.Length; ++i) {
            spheres[i] = transform.GetChild(i).gameObject;
            outlines[i] = spheres[i].GetComponent<Outline>();
        }
	}
	
	// Update is called once per frame
	void Update () {
        Debug.DrawRay(transform.position, transform.forward * 100, Color.red);
        Ray ray = new Ray(transform.position, transform.forward);

        // Closest to Ray
        int closestToRay = 0;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < spheres.Length; ++i) {
            Vector3 distance = Vector3.Cross(ray.direction, spheres[i].transform.position - ray.origin);
            if (distance.sqrMagnitude < closestDistance) {
                closestToRay = i;
                closestDistance = distance.sqrMagnitude;
            }
        }

        // Closest to Origin
        int closestToOrigin = 0;
        closestDistance = float.MaxValue;
        for (int i = 0; i < spheres.Length; ++i) {
            Vector3 distance = spheres[i].transform.position - ray.origin;
            if (distance.sqrMagnitude < closestDistance) {
                closestToOrigin = i;
                closestDistance = distance.sqrMagnitude;
            }
        }

        // Selected Closest
        int selectedClosest = -1;
        float closestDistanceToRay = float.MaxValue;
        float closestDistanceToOrigin = float.MaxValue;
        for (int i = 0; i < spheres.Length; ++i) {
            Vector3 distance = spheres[i].transform.position - ray.origin;
            Vector3 cross = Vector3.Cross(ray.direction, distance);
            if (Mathf.Abs(cross.sqrMagnitude - closestDistanceToRay) < UseOriginDistanceRange) {
                if (distance.sqrMagnitude < closestDistanceToOrigin) {
                    selectedClosest = i;
                    closestDistanceToRay = cross.sqrMagnitude;
                    closestDistanceToOrigin = distance.sqrMagnitude;
                }
            } else if (cross.sqrMagnitude < closestDistanceToRay) {
                selectedClosest = i;
                closestDistanceToRay = cross.sqrMagnitude;
                closestDistanceToOrigin = distance.sqrMagnitude;
            }
        }

        if (closestDistanceToRay > maxDistance) {
            selectedClosest = -1;
        }

        // Highlight
        Color c;
        string originMessage   = "Closest To Origin: ";
        string rayMessage      = "Closest To Ray   : ";
        string selectedMessage = "Selected         : ";

        for (int i = 0; i < spheres.Length; ++i) {
            c.a = 0;
            c.r = 0;
            c.g = 0;
            c.b = 0;

            if (i == closestToOrigin) {
                c.r = 1;
                c.a = 1;
                originMessage += spheres[i].name;
            }
            if (i == closestToRay) {
                c.g = 1;
                c.a = 1;
                rayMessage += spheres[i].name;
            }
            if (i == selectedClosest) {
                c.b = 1;
                c.a = 1;
                selectedMessage += spheres[i].name;
            }

            outlines[i].OutlineColor = c;
        }
        text.text = originMessage + "\n" + rayMessage + "\n" + selectedMessage;
    }
}
