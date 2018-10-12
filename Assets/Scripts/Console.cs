using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour {

    public Text Text;
    public bool DebugEnabled;

    private static Console instance;

	void Start () {
        instance = this;		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public static void Debug(string msg) {
        if (!instance || !instance.DebugEnabled) {
            return;
        }

        instance.Text.text = msg;
    }
}
