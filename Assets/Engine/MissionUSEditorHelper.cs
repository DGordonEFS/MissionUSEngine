using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

public class MissionUSEditorHelper
{
#if UNITY_EDITOR
    private Dictionary<string, int> _dropdowns = new Dictionary<string, int>();
    public float Zoom;

    private GenericMenu _menuToShow;

    public void Begin()
    {
        _menuToShow = null;
    }

    public void End()
    {
        if (_menuToShow != null)
            _menuToShow.ShowAsContext();
    }

    public void CreateDropdown(int index, List<string> choices, Action<int> callback, GUILayoutOption option = null)
    {
        CreateDropdown(null, index, choices, callback, option);
    }

    public void CreateDropdown(string label, int index, List<string> choices, Action<int> callback, GUILayoutOption option = null)
    {
        if (choices.Count == 0)
            return;

        GenericMenu.MenuFunction2 func = (object obj) =>
        {
            callback((int)obj);
        };

        if (label != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
        }
        
        GUIStyle style = "minipopup";
        bool wasClicked = false;

        

        if (option == null && GUILayout.Button(choices[index], style))
            wasClicked = true;
        else if (option != null && GUILayout.Button(choices[index], style, option))
            wasClicked = true;

        if (wasClicked)
        {
            var menu = new GenericMenu();
            for (int i = 0; i < choices.Count; i++)
            {
                menu.AddItem(new GUIContent(choices[i]), false, func, i);
            }

            _menuToShow = menu;
        }

        if (label != null)
            EditorGUILayout.EndHorizontal();
    }
#endif
}