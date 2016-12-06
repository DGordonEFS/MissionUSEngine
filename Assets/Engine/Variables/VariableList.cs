using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class VariableList : ISerializationCallbackReceiver
{
    [SerializeField]
    private List<string> _keys;
    [SerializeField]
    private List<VariableData> _values;

    public void OnBeforeSerialize()
    {
        _keys = _variables.Keys.ToList();
        _values = _variables.Values.ToList();
    }

    public void OnAfterDeserialize()
    {
        for (int i = 0; i < _keys.Count; i++)
            _variables.Add(_keys[i], _values[i]);
    }

    public VariableList Clone()
    {
        VariableList clone = new VariableList();

        foreach (var key in _variables.Keys)
        {
            var data = _variables[key];
            clone.SetVariable(key, data.Data, data.Type);
        }

        return clone;
    }

    public string Serialize()
    {
        return LitJson.JsonMapper.ToJson(_variables);
    }

    public void Deserialize(string data)
    {
        _variables = LitJson.JsonMapper.ToObject<Dictionary<string, VariableData>>(data);
    }

    public Dictionary<string, VariableData> _variables = new Dictionary<string, VariableData>();

    public List<string> GetKeys() { return _variables.Keys.ToList(); }

    public void RemoveVariable(string key)
    {
        key = key.ToUpper();
        if (_variables.ContainsKey(key))
            _variables.Remove(key);
    }

    public void SetVariable(string key, string value, VariableData.VariableTypes type = VariableData.VariableTypes.String)
    {
        _variables[key.ToUpper()] = new VariableData { Data = value, Type = type };
    }
    
    public void SetVariable(string key, int value)
    {
        _variables[key.ToUpper()] = new VariableData { Data = value.ToString(), Type = VariableData.VariableTypes.Number };
    }

    public void SetVariable(string key, float value)
    {
        _variables[key.ToUpper()] = new VariableData { Data = value.ToString(), Type = VariableData.VariableTypes.Number };
    }

    public void SetVariable(string key, bool value)
    {
        _variables[key.ToUpper()] = new VariableData { Data = value.ToString(), Type = VariableData.VariableTypes.Bool };
    }

    public VariableData.VariableTypes GetType(string key)
    {
        key = key.ToUpper();
        if (!_variables.ContainsKey(key))
            return default(VariableData.VariableTypes);

        return _variables[key].Type;
    }

    public string GetVariable(string key)
    {
        key = key.ToUpper();
        if (!_variables.ContainsKey(key))
            return "";

        return _variables[key.ToUpper()].Data;
    }

    public int GetInt(string key)
    {
        key = key.ToUpper();
        if (!_variables.ContainsKey(key))
            return 0;

        return int.Parse(_variables[key.ToUpper()].Data);
    }

    public float GetFloat(string key)
    {
        key = key.ToUpper();
        if (!_variables.ContainsKey(key))
            return 0;

        return float.Parse(_variables[key.ToUpper()].Data);
    }

    public bool GetBool(string key)
    {
        key = key.ToUpper();
        if (!_variables.ContainsKey(key))
            return false;

        return bool.Parse(_variables[key.ToUpper()].Data);
    }
}

[System.Serializable]
public class VariableData
{
    public enum VariableTypes { String, Number, Bool }
    public string Data;
    public VariableTypes Type;
}

public static class GlobalVariables
{
    private static VariableList _variables;
    public static VariableList GetInstance()
    {
        if (_variables == null)
            _variables = Resources.Load<VariableListSO>("GlobalVariables").Variables.Clone();
        return _variables;
    }
}

public class VariableListSO : ScriptableObject
{
    public VariableList Variables;
}