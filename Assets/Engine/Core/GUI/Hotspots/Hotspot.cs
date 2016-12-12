using UnityEngine;
using System.Collections;

public class Hotspot : MonoBehaviour {

    public string Id;

    public Script ShowScript = new Script();
    public Script HideScript = new Script();

    public Script EnterScript = new Script();
    public Script ExitScript = new Script();

    public Script ActivateScript = new Script();
    public Script UpdateScript = new Script();

    void OnDestroy()
    {

    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
