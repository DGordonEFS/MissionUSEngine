using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class SerialiazableDictionary<T, U> : Dictionary<T, U>, ISerializationCallbackReceiver {

    [SerializeField]
    private List<T> _keys = new List<T>();
    [SerializeField]
    private List<U> _values = new List<U>();

    public void OnBeforeSerialize()
    { 
        _keys = Keys.ToList();
        _values = Values.ToList();
    }

    public void OnAfterDeserialize()
    {
        Clear();
        for (int i = 0; i < _keys.Count; i++)
        {
            Add(_keys[i], _values[i]);
        }
    }
}
