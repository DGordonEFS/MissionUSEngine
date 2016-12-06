using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


using DarkChariotStudios.Dialogs;

public class FluxEditor : MissionUSEditorWindow
{


    [MenuItem("Mission US/History View")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<FluxEditor>();
        window.titleContent = new GUIContent("History View");
        window.Show();
        
        EditorApplication.update += window.OnUpdate;
    }

    private string _debugHistory;
    private bool _showInternalWaits;

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        Init();
    }

    void OnDestroy()
    {
        EditorApplication.update -= OnUpdate;
    }

    void OnUpdate()
    {
        Repaint();
    }

    private Vector2 _scrollPos;

    protected override void OnGUIDraw()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        List<string> history = AdventureEngine.GetInstance().GetHistory();


        _showInternalWaits = EditorGUILayout.Toggle("Internal Waits:", _showInternalWaits);
        if (!_showInternalWaits)
            history = history.FindAll((x) => !x.Contains(typeof(InternalWaitAction).Name));

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        for (int i = 0; i < history.Count; i++)
        { 
            var action = history[i];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(action, GUILayout.Height(25));
            if (GUILayout.Button("Run"))
            {
                AdventureEngine.GetInstance().Dispatch(action);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        
        
        EditorGUILayout.HelpBox("<b>Run Actions:</b> <i>Runs the actions written in the textarea below.</i>\n\n<b>Run History:</b> <i>Runs the history shown above.</i>", MessageType.Info);
        _debugHistory = EditorGUILayout.TextArea(_debugHistory);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy History To Clipboard"))
        {
            TextEditor te = new TextEditor();
            te.text = string.Join("\n", history.ToArray());
            te.SelectAll();
            te.Copy();
        }
        if (GUILayout.Button("Run Actions"))
        {
            GameObject.FindObjectOfType<MonoBehaviour>().StartCoroutine(RunAction(_debugHistory.Split('\n')));
        }
        if (GUILayout.Button("Run History"))
        {
            GameObject.FindObjectOfType<MonoBehaviour>().StartCoroutine(RunAction(history.ToArray()));
        }
        EditorGUILayout.EndHorizontal();

        
        

        
    }

    IEnumerator RunAction(string[] lines)
    {
        var trackHistory = AdventureEngine.GetInstance().TrackHistory;
        AdventureEngine.GetInstance().TrackHistory = false;
        for (int i = 0; i < lines.Length; i++)
            yield return AdventureEngine.GetInstance().Dispatch(lines[i]);
        AdventureEngine.GetInstance().TrackHistory = trackHistory;
    }
}
