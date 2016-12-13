using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

public class StateEditor : MissionUSEditorWindow<EditorPage, Memento<int>> {
    
    private static StateEditor _instance;
    public static StateEditor GetInstance()
    {
        if (_instance == null)
            Init();
        return _instance;
    }

    [MenuItem("Mission US/Data/StateEditor")]
    static void Init()
    {
        if (_instance != null)
            return;

        var window = EditorWindow.GetWindow<StateEditor>();
        window.titleContent = new GUIContent("State Editor");
        window.Show();

        OnScriptsReloaded();

        _instance = window;
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {

    }

    protected override void OnGUIDraw()
    {
        var hotspots = GameObject.FindObjectsOfType<Hotspot>();
        if (MUSEditor.EditorHelper.Button("Save"))
        {
            Debug.Log("save");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }


        EditorGUILayout.BeginVertical();

        for (int i = 0; i < hotspots.Length; i++)
        {
            var hotspot = hotspots[i];
            
            EditorGUILayout.BeginHorizontal();

            var newId = MUSEditor.EditorHelper.TextField(hotspot.Id, GUILayout.Width(100));
            
            if (newId != hotspot.Id)
            {
                hotspot.Id = newId;
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(50);
            EditorGUILayout.BeginVertical();
            if (MUSEditor.EditorHelper.Button("On Enter", GUILayout.Width(100)))
            {
                ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Owner=hotspot, Script = hotspot.EnterScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_enter" });
            }

            if (MUSEditor.EditorHelper.Button("On Exit", GUILayout.Width(100)))
            {
                ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Owner = hotspot, Script = hotspot.ExitScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_exit" });
            }

            if (MUSEditor.EditorHelper.Button("On Show", GUILayout.Width(100)))
            {
                ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Owner = hotspot, Script = hotspot.ShowScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_show" });
            }

            if (MUSEditor.EditorHelper.Button("On Hide", GUILayout.Width(100)))
            {
                ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Owner = hotspot, Script = hotspot.HideScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_hide" });
            }

            if (MUSEditor.EditorHelper.Button("On Activate", GUILayout.Width(100)))
            {
                ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Owner = hotspot, Script = hotspot.ActivateScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_activate" });
            }

            if (MUSEditor.EditorHelper.Button("On Update", GUILayout.Width(100)))
            {
                ScriptEditor.GetInstance().AddPage(new ScriptEditorPage() { Group = "hotspot", Owner = hotspot, Script = hotspot.UpdateScript, Id = "Hotspot: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + hotspot.Id + "_update" });
            }


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
}
