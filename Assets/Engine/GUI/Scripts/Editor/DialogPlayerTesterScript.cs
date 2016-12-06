using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(DialogPlayerTester))]
public class DialogPlayerTesterScript : Editor {

	public override void OnInspectorGUI()
    {
        var tester = (DialogPlayerTester)target;

        DrawDefaultInspector();

        if (tester.Dialog != null)
        {
            EditorGUILayout.LabelField("Num Nodes in Dialog: " + tester.Dialog.NumNodes);
            EditorGUILayout.LabelField("Default Node Id: " + tester.Dialog.DefaultNodeId);
        }

        if (!Application.isPlaying)
            return;

        if (tester.Dialog != null && GUILayout.Button("Start Dialog"))
        {
            tester.Open();
        }

        if (AdventureEngine.GetInstance().State.CurrentDialog != null && GUILayout.Button("Close Dialog"))
        {
            tester.Close();
        }
    }


}
