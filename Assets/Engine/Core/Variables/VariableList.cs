using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using LitJson;

using System.IO;

public class VariableList
{
    public static readonly List<string> BoolTypes = new List<string>() { "False", "True" };
    
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
        return JsonMapper.ToJson(_variables);
    }

    public void Deserialize(string data)
    {
        _variables = JsonMapper.ToObject<Dictionary<string, VariableData>>(data);
    }

    [JsonInclude]
    private Dictionary<string, VariableData> _variables = new Dictionary<string, VariableData>();

    public List<string> GetKeys() { return _variables.Keys.ToList(); }

    public void RemoveVariable(string key)
    {
        key = key.ToUpper();
        if (_variables.ContainsKey(key))
            _variables.Remove(key);
    }

    public bool HasKey(string key)
    {
        return _variables.ContainsKey(key.ToUpper());
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
        if (key == null)
            return default(VariableData.VariableTypes);

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
        {
            Refresh();
        }
        
        return _variables;
    }

    public const string PATH = "Assets/Engine/Data/Variables/Resources/";

    public static void Refresh()
    {
        if (_variables == null)
            _variables = new VariableList();

        var textAsset = Resources.Load<TextAsset>("GlobalVariables");
        if (textAsset == null)
        {
            Save();
        }
        var text = textAsset.text;
        _variables.Deserialize(text);
    }

    public static void Save()
    {
        var sr = File.CreateText(PATH + "GlobalVariables.txt");
        sr.Write(_variables.Serialize());
        sr.Close();
    }
}

public class VariableListSO : ScriptableObject
{
    public VariableList Variables;
}