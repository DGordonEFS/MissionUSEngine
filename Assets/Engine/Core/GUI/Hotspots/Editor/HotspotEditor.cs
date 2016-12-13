using UnityEngine;
using UnityEditor;

using System.Collections;

public class HotspotEditor : Editor {
/*
    SerializedProperty Id;
    SerializedProperty EnterScript;
    SerializedProperty ExitScript;
    SerializedProperty ShowScript;
    SerializedProperty HideScript;
    SerializedProperty ActivateScript;
    SerializedProperty UpdateScript;

    void OnEnable()
    {
        Id = serializedObject.FindProperty("Id");
        EnterScript = serializedObject.FindProperty("EnterScript");
        ExitScript = serializedObject.FindProperty("ExitScript");
        ShowScript = serializedObject.FindProperty("ShowScript");
        HideScript = serializedObject.FindProperty("HideScript");
        ActivateScript = serializedObject.FindProperty("ActivateScript");
        UpdateScript = serializedObject.FindProperty("UpdateScript");
    }
    */
    public override void OnInspectorGUI()
    {
        /*
        var hotspot = (Hotspot)target;

        serializedObject.Update();
        EditorGUILayout.PropertyField(Id);

       // hotspot.Id = MUSEditor.EditorHelper.TextField("Id:", hotspot.Id);

        if (MUSEditor.EditorHelper.Button("On Enter"))
        {
            ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Script = hotspot.EnterScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_enter" });
        }

        if (MUSEditor.EditorHelper.Button("On Exit"))
        {
            ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Script = hotspot.ExitScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_exit" });
        }

        if (MUSEditor.EditorHelper.Button("On Show"))
        {
            ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Script = hotspot.ShowScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_show"});
        }

        if (MUSEditor.EditorHelper.Button("On Hide"))
        {
            ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Script = hotspot.HideScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_hide"});
        }

        if (MUSEditor.EditorHelper.Button("On Activate"))
        {
            ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Script = hotspot.ActivateScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_activate" });
        }

        if (MUSEditor.EditorHelper.Button("On Update"))
        {
            ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Script = hotspot.UpdateScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_update" });
        }

        serializedObject.ApplyModifiedProperties();
        */
    }
}
