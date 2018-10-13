using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PauseEditor : MonoBehaviour {
	void Update () {
        if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftControl)) {
            Debug.Break();
        }
	}
}
