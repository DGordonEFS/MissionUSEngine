using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Script
{
    public static Dictionary<string, Type> BlockTypes;
    public static Dictionary<Type, string> BlockIdLookup;
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
    public enum BlockingTypes { No, Yes, Optional }
    public virtual BlockingTypes BlockingType { get { return BlockingTypes.No; } }


    public IEnumerator Execute(Script script)
    {
        yield return OnExecute(script);
    }

    protected virtual IEnumerator OnExecute(Script script)
    {
        yield return null;
    }

#if UNITY_EDITOR
    public List<ScriptBlock> OwnerBlocks;
    public float OnGUI()
    {
        var selectedType = Script.BlockIdLookup[GetType()];
        var newType = Script.BlockIds[UnityEditor.EditorGUILayout.Popup(Script.BlockIds.IndexOf(selectedType), Script.BlockIds.ToArray(), GUILayout.Width(100))];

        if (newType != selectedType)
        {
            int selfIndex = OwnerBlocks.IndexOf(this);
            OwnerBlocks.RemoveAt(selfIndex);
            OwnerBlocks.Insert(selfIndex, (ScriptBlock) Script.BlockTypes[newType].GetConstructor(new Type[0]).Invoke(new object[0]));
        }
        return OnDraw(75);
    }

    protected virtual float OnDraw(float height) { return height; }
#endif
}

[System.Serializable]
public class ScriptAction : ScriptBlock
{
}

[ScriptActionData(Id = "None")]
public class ScriptNoneAction : ScriptAction
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

[ScriptActionData(Id = "Wait")]
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
    protected override float OnDraw(float height)
    {
        Seconds = UnityEditor.EditorGUILayout.FloatField("Seconds:", Seconds);
        return height + 15;
    }

#endif
}

[ScriptActionData(Id = "Return")]
[System.Serializable]
public class ScriptReturnAction : ScriptAction
{
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

[ScriptActionData(Id = "ModifyVariable")]
[System.Serializable]
public class ScriptModifyVariableAction : ScriptAction
{
    public enum VariableTypes { GlobalVariable };
    public VariableTypes VariableType;
    public string VariableId;

    public enum ModifyTypes { Set, Increment, Decrement, Multiply, Divide }
    public ModifyTypes ModifyType;

    public enum ValueTypes { None, GlobalVariable };
    public ValueTypes ValueType;
    public string Value;

    protected override IEnumerator OnExecute(Script script)
    {
        VariableList variables = null;
        switch (VariableType)
        {
            case VariableTypes.GlobalVariable:
                variables = GlobalVariables.GetInstance();
                break;
        }

        string newValue = null;
        string oldValue = null;
        switch (ValueType)
        {
            case ValueTypes.None:
                newValue = Value;
                break;
            case ValueTypes.GlobalVariable:
                newValue = GlobalVariables.GetInstance().GetVariable(Value);
                oldValue = variables.GetVariable(VariableId);
                break;
        }
        

        switch (ModifyType)
        {
            case ModifyTypes.Set:
                variables.SetVariable(VariableId, newValue);
                break;
            case ModifyTypes.Increment:
                variables.SetVariable(VariableId, int.Parse(newValue) + int.Parse(oldValue));
                break;
            case ModifyTypes.Decrement:
                variables.SetVariable(VariableId, int.Parse(newValue) - int.Parse(oldValue));
                break;
            case ModifyTypes.Multiply:
                variables.SetVariable(VariableId, int.Parse(newValue) * int.Parse(oldValue));
                break;
            case ModifyTypes.Divide:
                variables.SetVariable(VariableId, int.Parse(newValue) / int.Parse(oldValue));
                break;
        }

        yield return null;
    }
}

public class ScriptContainer : ScriptAction { }

[ScriptActionData(Id = "If")]
[System.Serializable]
public class ScriptIfThenContainer : ScriptContainer
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }

    public ScriptIfThen If = new ScriptIfThen();
    public List<ScriptIfThen> ElseIf = new List<ScriptIfThen>();
    public ScriptIfThen Else;
}


[System.Serializable]
public class ScriptIfThen : ScriptBlock
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }

    public ScriptCondition Condition;
    public List<ScriptBlock> Then = new List<ScriptBlock>();
}

[ScriptActionData(Id = "While")]
[System.Serializable]
public class ScriptWhileContainer : ScriptContainer
{
    public override BlockingTypes BlockingType { get { return BlockingTypes.Yes; } }
    public ScriptIfThen While = new ScriptIfThen();
}


public class ScriptActionData : System.Attribute
{
    public string Id;
}