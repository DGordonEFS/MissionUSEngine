using UnityEngine;
using System.Collections;

public class DoNotDestroy : MonoBehaviour {

	// Use this for initialization
	void Awake () {
        GameObject.DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
