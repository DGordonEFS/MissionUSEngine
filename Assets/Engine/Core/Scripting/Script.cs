using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LitJson;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Script
{
    [NonSerialized]
    [JsonIgnore]
    public static Dictionary<string, string> BlockTypes;
    [NonSerialized]
    [JsonIgnore]
    public static Dictionary<string, string> BlockIdLookup;
    [NonSerialized]
    [JsonIgnore]
    public static Dictionary<string, string> BlockGroupLookup;
    [NonSerialized]
    [JsonIgnore]
    public static List<string> BlockIds;
    
    [SerializeField]
    [JsonInclude]
    private List<ScriptBlock> _blocks = new List<ScriptBlock>();
    [JsonIgnore]
    public List<ScriptBlock> Blocks { get { return _blocks; } }

    public ScriptBlock GetBlockAt(int index)
    {
        return _blocks[index];
    }


    [JsonIgnore]
    public int NumBlocks { get { return _blocks.Count; } }

    [JsonIgnore]
    public string ReturnValue { get; private set; }
    [JsonIgnore]
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

public abstract class ScriptBlock
{
    [NonSerialized]
    [JsonIgnore]
    public bool Used;

    [NonSerialized]
    [JsonIgnore]
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
    [NonSerialized]
    [JsonIgnore]
    public bool IsDisposed;

    [NonSerialized]
    [JsonIgnore]
    public List<ScriptBlock> OwnerBlocks;
    
    public void OnGUI(ScriptBlock prevBlock)
    {
        GUILayout.Space(2);

       // Debug.Log("count: " + GetType());
       // Debug.Log("id count: " + Script.BlockIdLookup.Count);
        //foreach (var id in Script.BlockIdLookup.Keys)
           // Debug.Log("   - " + id);
        //Debug.Log("id from type: " + Script.BlockIdLookup[GetType().Name]);

        var ids = Script.BlockIds.ToList();

        //Debug.Log("prev block: " + prevBlock + ", is: " + (prevBlock is ScriptIfThenContainer) + ", " + (prevBlock is ScriptElseIfThenContainer));
        
        if (!(prevBlock is ScriptIfThenContainer) && !(prevBlock is ScriptElseIfThenContainer))
        {
            ids.Remove("ElseIf");
            ids.Remove("Else");
        }

        var selectedType = Script.BlockIdLookup[GetType().Name];
        var index = ids.IndexOf(selectedType);
        if (index == -1)
            index = 0;

        EditorGUILayout.BeginHorizontal();

        MUSEditor.EditorHelper.Dropdown(index, ids.ConvertAll((x) => Script.BlockGroupLookup[x] + "/" + x), (value) =>
        {
            var newType = ids[value];
            if (newType != selectedType)
            {
                int selfIndex = OwnerBlocks.IndexOf(this);
                OwnerBlocks.RemoveAt(selfIndex);
                OwnerBlocks.Insert(selfIndex, (ScriptBlock)Type.GetType(Script.BlockTypes[newType]).GetConstructor(new Type[0]).Invoke(new object[0]));
            }
        }, GUILayout.Width(200));

        var offset = 0;
        if (this is ScriptContainer)
        {
            var container = this as ScriptContainer;
            if (container.Folded)
            {
                offset = 80;
                if (MUSEditor.EditorHelper.Button("Unfold", GUILayout.Width(60)))
                {
                    container.Folded = false;
                }
            }
            else
            {
                offset = 70;
                if (MUSEditor.EditorHelper.Button("Fold", GUILayout.Width(50)))
                {
                    container.Folded = true;
                }
            }
            
        }

        GUILayout.Space(170 - offset);
        if (MUSEditor.EditorHelper.Button("X"))
        {
            IsDisposed = true;
        }
        EditorGUILayout.EndHorizontal();

        
        GUILayout.Space(7);

        OnGUIDraw(prevBlock);

        GUILayout.Space(7);
        if (BlockingType == BlockingTypes.Optional)
        {
            MUSEditor.EditorHelper.Dropdown("IsBlocking:", IsBlocking ? 1 : 0, VariableList.BoolTypes, (value) => IsBlocking = value == 1);
        }
        
    }

    protected virtual void OnGUIDraw(ScriptBlock prevBlock) {  }
#endif
}

public class ScriptAction : ScriptBlock
{
}


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
        MUSEditor.EditorHelper.Dropdown("Visible:", Visible ? 1 : 0, VariableList.BoolTypes, (value) => Visible = value == 1);
    }

#endif
}

[ScriptActionData(Group="Flow", Id = "Return")]
public class ScriptReturnAction : ScriptAction
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.No; } }
    
    protected override IEnumerator OnExecute(Script script)
    {
        script.ApplyReturn(null);
        yield return null;
    }
}

[ScriptActionData(Group = "Variables", Id = "SetVariable")]
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


        variableList = GlobalVariables.GetInstance();
        keys = variableList.GetKeys();

        var group = "Global";

        int index = Mathf.Max(0, keys.IndexOf(VariableId)); // clamp to 0
        VariableId = keys[index];
        MUSEditor.EditorHelper.Dropdown(index, keys.ConvertAll((x) => group + "/" + variableList.GetType(x) + "/" + x), (value) =>
        {
            if (value != index)
                Value = null;

            VariableId = keys[value];
        }, GUILayout.Width(150));
      

        EditorGUILayout.LabelField("=", GUILayout.Width(15));
        

        var type = variableList.GetType(VariableId);

        group = "Global";

        var valueKeys = variableList.GetKeys().FindAll((x) => GlobalVariables.GetInstance().GetType(x) == type && x != VariableId); // only accept variables of the same type
        var valueIndex = Mathf.Max(0, valueKeys.IndexOf(Value)); // clamp to 0

        MUSEditor.EditorHelper.Dropdown(valueIndex, valueKeys.ConvertAll((x) => group + "/" + variableList.GetType(x) + "/" + x), (value) =>
        {
            Value = valueKeys[value];
        }, GUILayout.Width(150));
        
        EditorGUILayout.EndHorizontal();
    }
#endif

    protected override IEnumerator OnExecute(Script script)
    {
        yield return null;
    }
}

[ScriptActionData(Group = "Variables", Id = "SetVariableUser")]
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


        variableList = GlobalVariables.GetInstance();
        keys = variableList.GetKeys();

        var group = "Global";


        int index = Mathf.Max(0, keys.IndexOf(VariableId)); // clamp to 0
        VariableId = keys[index];
        MUSEditor.EditorHelper.Dropdown(index, keys.ConvertAll((x) => group + "/" + variableList.GetType(x) + "/" + x), (value) =>
        {
            VariableId = keys[value];

            if (value != index)
            {
                switch (variableList.GetType(VariableId))
                {
                    case VariableData.VariableTypes.String:
                        Value = "";
                        break;
                    case VariableData.VariableTypes.Number:
                        Value = "0";
                        break;
                    case VariableData.VariableTypes.Bool:
                        Value = "false";
                        break;
                }
            }
        }, GUILayout.Width(150));

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
            case VariableData.VariableTypes.Bool:
                MUSEditor.EditorHelper.Dropdown(bool.Parse(Value != null ? Value : "false") ? 1 : 0, VariableList.BoolTypes, (value) =>
                {
                    Value = (value == 1).ToString();
                }, GUILayout.Width(150));
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
    [NonSerialized]
    [JsonIgnore]
    public bool Folded;
    [NonSerialized]
    [JsonIgnore]
    public bool Entered;
    public ScriptIfThen Contents = new ScriptIfThen();
}

[ScriptActionData(Group="Flow", Id = "If")]
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
public class ScriptWhileContainer : ScriptContainer
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }
}


public class ScriptActionData : System.Attribute
{
    public string Group;
    public string Id;
}