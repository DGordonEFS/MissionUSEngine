using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using System.Linq;

public class ErrorEditor : MissionUSEditorWindow<EditorPage>
{
    [MenuItem ("Mission US/Debug/Error Panel")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<ErrorEditor>();
        window.titleContent = new GUIContent("Error Panel");
        window.Show();

        EditorApplication.update += window.OnUpdate;    
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        Init();
    }

    private int _frames;
    private VariableValidation VariableValidation = new VariableValidation();
    private DialogValidation DialogValidation = new DialogValidation();

    private Vector2 _scrollPos = Vector2.zero;

    void OnUpdate()
    {
        _frames++;
        if (_frames >= 100)
        {
            _frames = 0;
            Repaint();
        }
    }

    void OnDestroy()
    {
        EditorApplication.update -= OnUpdate;
    }


    protected override void OnGUIDraw()
    {
        List<DebugErrorMessage> errorMessages = new List<DebugErrorMessage>();

        errorMessages.AddRange(VariableValidation.Run().GetErrorMessages());
        errorMessages.AddRange(DialogValidation.Run().GetErrorMessages());

        EditorGUILayout.SelectableLabel("Num Errors: " + errorMessages.Count);
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        for (int i = 0; i < errorMessages.Count; i++)
        {
            var error = errorMessages[i];
            EditorGUILayout.SelectableLabel(error.From + " ::: " + error.Message);
        }
        EditorGUILayout.EndScrollView();
    }
}

public class DebugErrorMessage
{
    public string From { get; private set; }
    public string Message { get; private set; }

    public DebugErrorMessage(string from, string message)
    {
        From = from;
        Message = message;
    }
}


public class Validation
{
    private List<DebugErrorMessage> _errorMessages = new List<DebugErrorMessage>();
    public List<DebugErrorMessage> GetErrorMessages() { return _errorMessages.ToList(); }

    public void AddErrorMessage(DebugErrorMessage errorMessage)
    {
        _errorMessages.Add(errorMessage);
    }

    public Validation Run()
    {
        _errorMessages.Clear();
        OnRun();
        return this;
    }

    protected virtual void OnRun() { }
}

public class VariableValidation : Validation
{
    protected override void OnRun()
    {
        var so = Resources.Load<VariableListSO>("GlobalVariables");
        if (so == null)
            return;

        var globalVariables = so.Variables;

        var keys = globalVariables.GetKeys();
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            var type = globalVariables.GetType(key);
            var value = globalVariables.GetVariable(key);
            
            bool hasError = false;
            switch (type)
            {
                case VariableData.VariableTypes.Number:
                    float floatVal;
                    if (!float.TryParse(value, out floatVal))
                        hasError = true;
                    break;
                case VariableData.VariableTypes.Bool:
                    bool boolVal;
                    if (!bool.TryParse(value, out boolVal))
                        hasError = true;
                    break;
            }

            if (hasError)
                AddErrorMessage(new DebugErrorMessage("Variable", "Variable \"" + key + "\" of type \"" + System.Enum.GetName(typeof(VariableData.VariableTypes), type) + "\" has an invalid value of \"" + value + "\""));
        }
    }
}

public class DialogValidation : Validation
{

}