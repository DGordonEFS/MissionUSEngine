using UnityEngine;
using System.Collections.Generic;
using System;

using LitJson;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ScriptCondition
{
    [NonSerialized]
    [JsonIgnore]
    public static Dictionary<string, string> ConditionTypes;
    [NonSerialized]
    [JsonIgnore]
    public static Dictionary<string, string> ConditionIdLookup;
    [NonSerialized]
    [JsonIgnore]
    public static Dictionary<string, string> ConditionGroupLookup;
    [NonSerialized]
    [JsonIgnore]
    public static List<string> ConditionIds;


    public List<ScriptConditionBlock> ConditionBlocks = new List<ScriptConditionBlock>();

    public bool Evaluate(Script script)
    {
        for (int i = 0; i < ConditionBlocks.Count; i++)
            if (ConditionBlocks[i].Evaluate())
                return true;

        return false;
    }

#if UNITY_EDITOR
    public void OnGUIDraw(ScriptContainer container)
    {
        GUILayout.Space(25);
        for (var i = 0; i < ConditionBlocks.Count; i++)
        {
            var conditionBlock = ConditionBlocks[i];

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(200);
            EditorGUILayout.LabelField("Group " + (i + 1), GUILayout.Width(75));

            if (MUSEditor.EditorHelper.Button("X"))
            {
                container.CreateMemento();
                ConditionBlocks.Remove(conditionBlock);
            }

            EditorGUILayout.EndHorizontal();
            
            conditionBlock.OnGUIDraw(container);
            
            GUILayout.Space(25);
        }

        if (MUSEditor.EditorHelper.Button("New Group"))
        {
            container.CreateMemento();
            ConditionBlocks.Add(new ScriptConditionBlock());
        }
    }
#endif
}

[Serializable]
public class ScriptConditionBlock
{
    public List<ScriptConditionExpression> Expressions = new List<ScriptConditionExpression>();

    public bool Evaluate()
    {
        for (int i = 0; i < Expressions.Count; i++)
            if (!Expressions[i].Evaluate())
                return false;

        return true;
    }

    public void OnGUIDraw(ScriptContainer container)
    {
        for (var j = 0; j < Expressions.Count; j++)
        {
            var expression = Expressions[j];
            expression.GUIDraw(container, Expressions);
        }

        EditorGUILayout.BeginHorizontal();
        //GUILayout.Space(25);
        if (MUSEditor.EditorHelper.Button("New Expression"))
        {
            container.CreateMemento();
            Expressions.Add(new ExpressionVariable());
        }
        EditorGUILayout.EndHorizontal();
    }
}

[Serializable]
public abstract class ScriptConditionExpression
{
    public static readonly List<string> ExpressionTypes = new List<string>() { "=", ">=", "<=", ">", "<" };

    public bool Evaluate()
    {
        return OnEvaluate();
    }

    protected virtual bool OnEvaluate()
    {
        return true;
    }

#if UNITY_EDITOR
    public void GUIDraw(ScriptContainer container, List<ScriptConditionExpression> ownerExpressions)
    {
        EditorGUILayout.BeginHorizontal();

       // GUILayout.Space(25);
        var ids = ScriptCondition.ConditionIds.ToList();
        var selectedType = ScriptCondition.ConditionIdLookup[GetType().Name];
        var index = ids.IndexOf(selectedType);
        if (index == -1)
            index = 0;

        MUSEditor.EditorHelper.Dropdown(index, ids.ConvertAll((x) => ScriptCondition.ConditionGroupLookup[x] + "/" + x), (value) =>
        {
            var newType = ids[value];
            if (newType != selectedType)
            {
                container.CreateMemento();

                int selfIndex = ownerExpressions.IndexOf(this);
                ownerExpressions.RemoveAt(selfIndex);

                var newBlock = (ScriptConditionExpression)Type.GetType(ScriptCondition.ConditionTypes[newType]).GetConstructor(new Type[0]).Invoke(new object[0]);
                ownerExpressions.Insert(selfIndex, newBlock);
            }
        }, GUILayout.Width(20));


        OnGUIDraw();


        EditorGUILayout.Space();
        if (MUSEditor.EditorHelper.Button("X"))
        {
            container.CreateMemento();
            ownerExpressions.Remove(this);
        }
        EditorGUILayout.EndHorizontal();
    }

    protected virtual void OnGUIDraw()
    {

    }
#endif
}


[ScriptConditionData(Group ="Variables", Id="Test")]
public class ExpressionVariable : ScriptConditionExpression
{
    public string VariableAId;
    public string ExpressionType;
    public string VariableBId;

#if UNITY_EDITOR
    protected override void OnGUIDraw()
    {
        VariableList variableList = null;
        var keys = new List<string>();


        variableList = GlobalVariables.GetInstance();
        keys = variableList.GetKeys();

        var group = "Global";

        int index = Mathf.Max(0, keys.IndexOf(VariableAId)); // clamp to 0
        VariableAId = keys[index];
        MUSEditor.EditorHelper.Dropdown(index, keys.ConvertAll((x) => group + "/" + variableList.GetType(x) + "/" + x), (value) =>
        {
            if (value != index)
                VariableBId = null;

            VariableAId = keys[value];
        }, GUILayout.Width(150));


        index = ScriptConditionExpression.ExpressionTypes.IndexOf(ExpressionType);
        if (index < 0)
            index = 0;
        MUSEditor.EditorHelper.Dropdown(index, ScriptConditionExpression.ExpressionTypes, (selectedIndex) =>
        {
            ExpressionType = ScriptConditionExpression.ExpressionTypes[selectedIndex];
        }, GUILayout.Width(40));

        var type = variableList.GetType(VariableBId);

        group = "Global";

        var valueKeys = variableList.GetKeys().FindAll((x) => GlobalVariables.GetInstance().GetType(x) == type && x != VariableAId); // only accept variables of the same type
        var valueIndex = Mathf.Max(0, valueKeys.IndexOf(VariableBId)); // clamp to 0

        MUSEditor.EditorHelper.Dropdown(valueIndex, valueKeys.ConvertAll((x) => group + "/" + variableList.GetType(x) + "/" + x), (value) =>
        {
            VariableBId = valueKeys[value];
        }, GUILayout.Width(150));
    }
#endif
}

[ScriptConditionData(Group = "Variables", Id = "TestUser")]
public class ExpressionVariableUser : ScriptConditionExpression
{
    public string VariableAId;
    public string ExpressionType;
    public string Value;

#if UNITY_EDITOR
    protected override void OnGUIDraw()
    {
        VariableList variableList = null;
        var keys = new List<string>();


        variableList = GlobalVariables.GetInstance();
        keys = variableList.GetKeys();

        var group = "Global";

        int index = Mathf.Max(0, keys.IndexOf(VariableAId)); // clamp to 0
        VariableAId = keys[index];
        MUSEditor.EditorHelper.Dropdown(index, keys.ConvertAll((x) => group + "/" + variableList.GetType(x) + "/" + x), (value) =>
        {
            if (value != index)
                Value = null;

            VariableAId = keys[value];
        }, GUILayout.Width(150));


        index = ScriptConditionExpression.ExpressionTypes.IndexOf(ExpressionType);
        if (index < 0)
            index = 0;
        MUSEditor.EditorHelper.Dropdown(index, ScriptConditionExpression.ExpressionTypes, (selectedIndex) =>
        {
            ExpressionType = ScriptConditionExpression.ExpressionTypes[selectedIndex];
        });

        var type = variableList.GetType(VariableAId);

        switch (type)
        {
            case VariableData.VariableTypes.String:
                Value = EditorGUILayout.TextArea(Value, GUILayout.MinWidth(155));
                break;
            case VariableData.VariableTypes.Number:
                if (Value == null)
                    Value = "0";
                Value = EditorGUILayout.FloatField(float.Parse(Value), GUILayout.Width(150)).ToString();
                break;
            case VariableData.VariableTypes.Bool:
                MUSEditor.EditorHelper.Dropdown(bool.Parse(Value != null ? Value : "false") ? 1 : 0, VariableList.BoolTypes, (value) =>
                {
                    Value = (value == 1).ToString();
                }, GUILayout.Width(150));
                break;
        }
    }
#endif
}