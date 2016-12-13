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
    public float Zoom { get; private set; }

    private GenericMenu _menuToShow;

    public void Begin(float zoom)
    {
        _menuToShow = null;
        Zoom = zoom;
    }

    public void End()
    {
        if (_menuToShow != null)
            _menuToShow.ShowAsContext();

      //  Zoom = 1;
    }

    public string TextField(string text)
    {
        return EditorGUILayout.TextField(text);
    }

    public string TextField(string text, GUILayoutOption option)
    {
        return EditorGUILayout.TextField(text, option);
    }

    public string TextField(string text, GUIStyle style)
    {
        return EditorGUILayout.TextField(text, style);
    }

    public string TextField(string prefix, string text)
    {
        return EditorGUILayout.TextField(prefix, text);
    }

    public string TextField(string prefix, string text, GUIStyle style)
    {
        return EditorGUILayout.TextField(prefix, text, style);
    }

    public bool Button(string text)
    {
        return GUILayout.Button(text);
    }

    public bool Button(string text, GUIStyle style)
    {
        return GUILayout.Button(text, style);
    }

    public bool Button(string text, GUIStyle style, GUILayoutOption option)
    {
        return GUILayout.Button(text, style, option);
    }

    public bool Button(string text, GUILayoutOption option)
    {
        return GUILayout.Button(text, option);
    }

    public void Dropdown(int index, List<string> choices, Action<int> callback, GUILayoutOption option = null)
    {
        Dropdown(null, index, choices, callback, option);
    }

    public void Dropdown(string label, int index, List<string> choices, Action<int> callback, GUILayoutOption option = null)
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
            Debug.Log(" show menu");
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