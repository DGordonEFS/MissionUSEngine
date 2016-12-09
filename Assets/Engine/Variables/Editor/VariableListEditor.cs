using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class VariableEditorPage : EditorPage
{

}

public class VariableListEditor : MissionUSEditorWindow<VariableEditorPage>
{
    [MenuItem("Mission US/Data/VariableEditor")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<VariableListEditor>();
        window.titleContent = new GUIContent("Variable Editor");
        window.Show();
        window.TopBar = true;
        window.MainArea = true;

        OnScriptsReloaded();
    }

    

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
    }

    protected override void OnDrawTopBar()
    {
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        if (MUSEditor.EditorHelper.Button("Save"))
        {
            GlobalVariables.Save();
        }
        if (MUSEditor.EditorHelper.Button("Refresh"))
        {
            GlobalVariables.Refresh();
        }
        EditorGUILayout.EndHorizontal();
    }

    void OnDestroy()
    {
    }

    protected override void OnDrawMainArea()
    {
        GUILayout.BeginArea(new Rect(5, 0, Screen.width - 10, Screen.height));
        GUILayout.Space(5);
        var variables = GlobalVariables.GetInstance();
        

        var keys = variables.GetKeys();
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];

            EditorGUILayout.BeginHorizontal();

            var type = variables.GetType(key);
            var value = variables.GetVariable(key);
            
            switch (type)
            {
                case VariableData.VariableTypes.Number:
                    float floatVal;
                    if (!float.TryParse(value, out floatVal))
                        value = "0";
                    break;
                case VariableData.VariableTypes.Bool:
                    bool boolVal;
                    if (!bool.TryParse(value, out boolVal))
                        value = "false";
                    break;
            }
            
            var newType = EditorGUILayout.EnumPopup(type);
            var newKey = EditorGUILayout.TextField(key);

            string newValue = null;

            
            switch (type)
            {
                case VariableData.VariableTypes.String:
                    newValue = EditorGUILayout.TextField(value);
                    break;
                case VariableData.VariableTypes.Number:
                    newValue = EditorGUILayout.IntField(int.Parse(value)).ToString();
                    break;
                case VariableData.VariableTypes.Bool:
                    newValue = value;
                    MUSEditor.EditorHelper.Dropdown(bool.Parse(value) ? 1 : 0, VariableList.BoolTypes, (val) => {
                        if (variables.HasKey(key))
                            variables.SetVariable(newKey, (val == 1).ToString(), VariableData.VariableTypes.Bool);
                    }, GUILayout.Width(105));
                    break;
            }


            variables.RemoveVariable(key);

            

            if (!MUSEditor.EditorHelper.Button("X"))
                variables.SetVariable(newKey, newValue, (VariableData.VariableTypes)newType);

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);
        }

        if (MUSEditor.EditorHelper.Button("Add Variable"))
        {
            variables.SetVariable("", "");
        }

        GUILayout.EndArea();
    }
}
