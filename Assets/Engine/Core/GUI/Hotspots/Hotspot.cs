using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[ExecuteInEditMode]
public class Hotspot : MonoBehaviour, ISerializationCallbackReceiver
{

    public string Id;
    
    public Script ShowScript = new Script();
    public Script HideScript = new Script();
    
    public Script EnterScript = new Script();
    public Script ExitScript = new Script();
    
    public Script ActivateScript = new Script();
    public Script UpdateScript = new Script();

    [SerializeField]
    private List<string> _serializedData = new List<string>();

    void OnDestroy()
    {

    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
    }


    public void OnBeforeSerialize()
    {
        _serializedData.Clear();
        _serializedData.Add(Id);
        _serializedData.Add(MUSEditor.SerializeObject<Script>(EnterScript));
        _serializedData.Add(MUSEditor.SerializeObject<Script>(ExitScript));
        _serializedData.Add(MUSEditor.SerializeObject<Script>(ShowScript));
        _serializedData.Add(MUSEditor.SerializeObject<Script>(HideScript));
        _serializedData.Add(MUSEditor.SerializeObject<Script>(ActivateScript));
        _serializedData.Add(MUSEditor.SerializeObject<Script>(UpdateScript));
    }

    public void OnAfterDeserialize()
    {
        Id = _serializedData[0];
        EnterScript = MUSEditor.DeserializeObject<Script>(_serializedData[1]);
        ExitScript = MUSEditor.DeserializeObject<Script>(_serializedData[2]);
        ShowScript = MUSEditor.DeserializeObject<Script>(_serializedData[3]);
        HideScript = MUSEditor.DeserializeObject<Script>(_serializedData[4]);
        ActivateScript = MUSEditor.DeserializeObject<Script>(_serializedData[5]);
        UpdateScript = MUSEditor.DeserializeObject<Script>(_serializedData[6]);
        _serializedData.Clear();
    }
}
