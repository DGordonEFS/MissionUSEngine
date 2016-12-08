using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class Script
{
    public static VariableList GlobalVariables;
    public static Dictionary<string, Type> BlockTypes;
    public static Dictionary<Type, string> BlockIdLookup;
    public static Dictionary<string, string> BlockGroupLookup;
    public static List<string> BlockIds;

    [HideInInspector]
    [SerializeField]
    private List<ScriptBlock> _blocks = new List<ScriptBlock>();
    public List<ScriptBlock> Blocks { get { return _blocks; } }
    public ScriptBlock GetBlockAt(int index)
    {
        return _blocks[index];
    }


    public int NumBlocks { get { return _blocks.Count; } }

    public string ReturnValue { get; private set; }
    public bool IsRunning { get; private set; }

    public void ApplyReturn(string value)
    {
        ReturnValue = value;
        IsRunning = false;
    }

    public IEnumerator Execute()
    {
        for (int i = 0; i < _blocks.Count; i++)
        {
            yield return _blocks[i].Execute(this);
            if (!IsRunning)
                break;
        }
    }
}

[System.Serializable]
public class ScriptBlock
{
    public bool Used;

    public Vector2 EditorSize = new Vector2(400, 60);

    public enum BlockingTypes { No, Yes, Optional }
    public virtual BlockingTypes BlockingType { get { return BlockingTypes.Optional; } }

    public bool IsBlocking;

    public IEnumerator Execute(Script script)
    {
        yield return OnExecute(script);
    }

    protected virtual IEnumerator OnExecute(Script script)
    {
        yield return null;
    }

#if UNITY_EDITOR
    public bool IsDisposed;
    public List<ScriptBlock> OwnerBlocks;
    
    public void OnGUI(ScriptBlock prevBlock)
    {
        GUILayout.Space(2);


        var ids = Script.BlockIds.ToList();

        //Debug.Log("prev block: " + prevBlock + ", is: " + (prevBlock is ScriptIfThenContainer) + ", " + (prevBlock is ScriptElseIfThenContainer));
        
        if (!(prevBlock is ScriptIfThenContainer) && !(prevBlock is ScriptElseIfThenContainer))
        {
            ids.Remove("ElseIf");
            ids.Remove("Else");
        }

        var selectedType = Script.BlockIdLookup[GetType()];
        var index = ids.IndexOf(selectedType);
        if (index == -1)
            index = 0;

        EditorGUILayout.BeginHorizontal();
        var newType = ids[UnityEditor.EditorGUILayout.Popup(index, ids.ConvertAll((x)=>Script.BlockGroupLookup[x] + "/" + x).ToArray(), GUILayout.Width(200))];

        var offset = 0;
        if (this is ScriptContainer)
        {
            var container = this as ScriptContainer;
            if (container.Folded)
            {
                offset = 80;
                if (GUILayout.Button("Unfold", GUILayout.Width(60)))
                {
                    container.Folded = false;
                }
            }
            else
            {
                offset = 70;
                if (GUILayout.Button("Fold", GUILayout.Width(50)))
                {
                    container.Folded = true;
                }
            }
            
        }

        GUILayout.Space(170 - offset);
        if (GUILayout.Button("X"))
        {
            IsDisposed = true;
        }
        EditorGUILayout.EndHorizontal();

        if (newType != selectedType)
        {
            int selfIndex = OwnerBlocks.IndexOf(this);
            OwnerBlocks.RemoveAt(selfIndex);
            OwnerBlocks.Insert(selfIndex, (ScriptBlock) Script.BlockTypes[newType].GetConstructor(new Type[0]).Invoke(new object[0]));
        }
        GUILayout.Space(7);

        OnGUIDraw(prevBlock);

        if (BlockingType == BlockingTypes.Optional)
        {
            MUSEditor.EditorHelper.CreateDropdown("IsBlocking:", IsBlocking ? 1 : 0, VariableList.BoolTypes, (value) => IsBlocking = value == 1);
        }
        
    }

    protected virtual void OnGUIDraw(ScriptBlock prevBlock) {  }
#endif
}

[System.Serializable]
public class ScriptAction : ScriptBlock
{
}


[System.Serializable]
public class ScriptCondition : ScriptBlock
{
    public string Evaluate(Script script)
    {
        return OnEvaluate(script);
    }

    protected virtual string OnEvaluate(Script script)
    {
        return null;
    }
}

[ScriptActionData(Group="Flow", Id = "Wait")]
[System.Serializable]
public class ScriptWaitAction : ScriptAction
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }
    public float Seconds;

    protected override IEnumerator OnExecute(Script script)
    {
        yield return new WaitForSeconds(Seconds);
    }

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        Seconds = UnityEditor.EditorGUILayout.FloatField("Seconds:", Seconds);
    }

#endif
}


public class ScriptHotspotAction : ScriptAction
{
    public string Id;

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        Id = EditorGUILayout.TextField("Id:", Id);
    }

#endif
}

[ScriptActionData(Group="Hotspots", Id = "Fade")]
[System.Serializable]
public class ScriptHotspotFadeAction : ScriptHotspotAction
{
    public float Alpha;
    public float Seconds;

    protected override IEnumerator OnExecute(Script script)
    {
        yield return new WaitForSeconds(Seconds);
    }

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        Alpha = EditorGUILayout.FloatField("Alpha:", Alpha);
        Seconds = EditorGUILayout.FloatField("Seconds:", Seconds);
    }

#endif
}

[ScriptActionData(Group = "Hotspots", Id = "Visible")]
[System.Serializable]
public class ScriptHotspotVisibleAction : ScriptHotspotAction
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.No; } }

    public bool Visible;

    protected override IEnumerator OnExecute(Script script)
    {
        yield return null;
    }

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        MUSEditor.EditorHelper.CreateDropdown("Visible:", Visible ? 1 : 0, VariableList.BoolTypes, (value) => Visible = value == 1);
    }

#endif
}

[ScriptActionData(Group="Flow", Id = "Return")]
[System.Serializable]
public class ScriptReturnAction : ScriptAction
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.No; } }

    public enum ValueTypes { None, GlobalVariable, Condition };
    public ValueTypes ValueType;
    public object Value;

    protected override IEnumerator OnExecute(Script script)
    {
        string value = null;
        switch (ValueType)
        {
            case ValueTypes.None:
                value = Value.ToString();
                break;
            case ValueTypes.GlobalVariable:
                value = GlobalVariables.GetInstance().GetVariable(Value.ToString());
                break;
            case ValueTypes.Condition:
                value = ((ScriptCondition)Value).Evaluate(script);
                break;
        }

        script.ApplyReturn(value);
        yield return null;
    }
}

[ScriptActionData(Group = "Variables", Id = "SetVariable")]
[System.Serializable]
public class ScriptSetVariableAction : ScriptAction
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.No; } }

    public string VariableId;
    
    public string Value;

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        EditorGUILayout.BeginHorizontal();
        VariableList variableList = null;
        var keys = new List<string>();


        variableList = Script.GlobalVariables;
        keys = variableList.GetKeys();

        var group = "Global";

        int index = Mathf.Max(0, keys.IndexOf(VariableId)); // clamp to 0
        var newIndex = EditorGUILayout.Popup(index, keys.ConvertAll((x)=>group + "/" + x).ToArray(), GUILayout.Width(150));
        VariableId = keys[newIndex];

        if (index != newIndex)
            Value = null;

        EditorGUILayout.LabelField("=", GUILayout.Width(15));
        
        var type = variableList.GetType(VariableId);

        group = "Global";

        keys = variableList.GetKeys().FindAll((x) => Script.GlobalVariables.GetType(x) == type && x != VariableId); // only accept variables of the same type
        index = Mathf.Max(0, keys.IndexOf(Value)); // clamp to 0
        newIndex = EditorGUILayout.Popup(index, keys.ConvertAll((x)=>group + "/" + x).ToArray(), GUILayout.Width(150));
        Value = keys[newIndex];
        EditorGUILayout.EndHorizontal();
    }
#endif

    protected override IEnumerator OnExecute(Script script)
    {
        yield return null;
    }
}

[ScriptActionData(Group = "Variables", Id = "SetVariableUser")]
[System.Serializable]
public class ScriptSetVariableUserAction : ScriptAction
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.No; } }

    public string VariableId;
    
    public string Value;

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        EditorGUILayout.BeginHorizontal();
        VariableList variableList = null;
        var keys = new List<string>();


        variableList = Script.GlobalVariables;
        keys = variableList.GetKeys();

        var group = "Global";

        int index = Mathf.Max(0, keys.IndexOf(VariableId)); // clamp to 0
        var newIndex = EditorGUILayout.Popup(index, keys.ConvertAll((x)=>group + "/" + x).ToArray(), GUILayout.Width(150));
        //Debug.Log("index: " + index + ", newIndex: " + newIndex + ", total: " + keys.Count);
        VariableId = keys[newIndex];

        if (index != newIndex)
            Value = null;

        EditorGUILayout.LabelField("=", GUILayout.Width(15));

        var type = variableList.GetType(VariableId);

        switch (type)
        {
            case VariableData.VariableTypes.String:
                Value = EditorGUILayout.TextArea(Value);
                break;
            case VariableData.VariableTypes.Number:
                if (Value == null)
                    Value = "0";
                Value = EditorGUILayout.FloatField(float.Parse(Value)).ToString();
                break;
        }
        EditorGUILayout.EndHorizontal();
    }
#endif

    protected override IEnumerator OnExecute(Script script)
    {
        yield return null;
    }
}

public abstract class ScriptContainer : ScriptAction
{
    public bool Folded;
    public bool Entered;
    public ScriptIfThen Contents = new ScriptIfThen();
}

[ScriptActionData(Group="Flow", Id = "If")]
[System.Serializable]
public class ScriptIfThenContainer : ScriptContainer
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        GUILayout.Space(50);
    }
#endif
}

[ScriptActionData(Group = "Flow", Id = "ElseIf")]
[System.Serializable]
public class ScriptElseIfThenContainer : ScriptContainer
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }
#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        GUILayout.Space(50);
    }
#endif
}

[ScriptActionData(Group = "Flow", Id = "Else")]
[System.Serializable]
public class ScriptElseThenContainer : ScriptContainer
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }

#if UNITY_EDITOR
    protected override void OnGUIDraw(ScriptBlock prevBlock)
    {
        GUILayout.Space(50);
    }
#endif
}

[System.Serializable]
public class ScriptIfThen
{
    public ScriptCondition Condition;
    public List<ScriptBlock> Then = new List<ScriptBlock>();
}

[ScriptActionData(Id = "While")]
[System.Serializable]
public class ScriptWhileContainer : ScriptContainer
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }
}


public class ScriptActionData : System.Attribute
{
    public string Group;
    public string Id;
}