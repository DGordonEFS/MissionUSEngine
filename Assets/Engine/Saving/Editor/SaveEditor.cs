using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using DarkChariotStudios.Dialogs;

public class SaveEditor : MissionUSEditorWindow<EditorPage>
{
    [MenuItem("Mission US/Save Game Editor")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<SaveEditor>();
        window.titleContent = new GUIContent("Save Games");
        window.Show();
        window._saveId = null;
        window._loadIndex = 0;
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        Init();
    }

    private string _saveId;
    private int _saveAsIndex;
    private int _loadIndex;

    private Vector2 _scrollPos;

    protected override void OnGUIDraw()
    {
        if (Application.isPlaying)
        {
            List<string> saveIds = new List<string>(AssetDatabase.FindAssets("t:SaveData")).ConvertAll<string>((x) =>
            {
                var path = AssetDatabase.GUIDToAssetPath(x);
                return path.Substring(path.LastIndexOf("/") + 6, path.IndexOf(".asset") - (path.LastIndexOf("/") + 6));
            });



            EditorGUILayout.HelpBox("Create a new save game.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Save Id:", GUILayout.Width(75));
            _saveId = EditorGUILayout.TextField(_saveId);
            if (!saveIds.Contains(_saveId) && GUILayout.Button("New Save"))
            {
                SaveData data = new SaveData();
                data.Variables = GlobalVariables.GetInstance().Serialize();
                AssetDatabase.CreateAsset(data, "Assets/Engine/Data/Saves/Resources/save_" + _saveId + ".asset");
                _saveId = null;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (saveIds.Count > 0)
            {
                EditorGUILayout.HelpBox("Overwrite a save game.", MessageType.Info);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Games:", GUILayout.Width(75));
                _saveAsIndex = EditorGUILayout.Popup(_saveAsIndex, saveIds.ToArray());
                if (GUILayout.Button("Save As"))
                {
                    SaveData data = new SaveData();
                    data.Variables = GlobalVariables.GetInstance().Serialize();
                    Debug.Log("Save variables: " + data.Variables);
                    AssetDatabase.CreateAsset(data, "Assets/Engine/Data/Saves/Resources/save_" + saveIds[_saveAsIndex] + ".asset");
                    _saveId = null;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Load a saved game.", MessageType.Info);
            _loadIndex = EditorGUILayout.Popup("Games:", _loadIndex, saveIds.ToArray());
            if (GUILayout.Button("Load"))
            {
                EditorApplication.isPlaying = true;

                Debug.Log("load: " + "save_" + saveIds[_loadIndex]);
                var saveData = Resources.Load<SaveData>("save_" + saveIds[_loadIndex]);

                

                Debug.Log("launch app");
            }
        }
        else
        {
            string[] paths = AssetDatabase.FindAssets("t:SaveData");
            List<string> saveIds = new List<string>(paths).ConvertAll<string>((x) =>
            {
                var path = AssetDatabase.GUIDToAssetPath(x);
                return path.Substring(path.LastIndexOf("/") + 1, path.IndexOf(".asset") - (path.LastIndexOf("/")+1));
            });
            EditorGUILayout.Popup("Games:", 0, saveIds.ToArray());

        }


    }
}
