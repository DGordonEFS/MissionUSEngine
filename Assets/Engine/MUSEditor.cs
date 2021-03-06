﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

using LitJson;

public class MementoStack<T>
{
    private List<Memento<T>> _undo = new List<Memento<T>>();
    private List<Memento<T>> _redo = new List<Memento<T>>();

    public Action<T> Restore = (T) => { };
    public Func<T> GetMementoData = () => { return default(T); };
    public Action OnChange = () => { };


    public void CreateMemento()
    {
        _undo.Add(new Memento<T>(GetMementoData()));
        _redo.Clear();

        OnChange();
    }

    public void Undo()
    {
        if (_undo.Count == 0)
            return;

        var memento = _undo[_undo.Count - 1];
        _undo.Remove(memento);

        _redo.Add(new Memento<T>(GetMementoData()));

        Restore(memento.GetData());
        OnChange();
    }

    public void Redo()
    {
        if (_redo.Count == 0)
            return;

        var memento = _redo[_redo.Count - 1];
        _redo.Remove(memento);

        _undo.Add(new Memento<T>(GetMementoData()));
        Restore(memento.GetData());
        OnChange();
    }
}

public class Memento<T>
{
    private string _data;

    public Memento(T data)
    {
        var sb = new System.Text.StringBuilder();
        var writer = new JsonWriter(sb);
        writer.TypeHinting = true;
        JsonMapper.ToJson(data, writer);
        _data = sb.ToString();
    }

    public T GetData()
    {
        var reader = new JsonReader(_data);
        reader.TypeHinting = true;
        return JsonMapper.ToObject<T>(reader);
    }
}

[InitializeOnLoad]
public class MUSEditor {
    
    private static MissionUSEditorHelper _editorHelper = new MissionUSEditorHelper();
    public static MissionUSEditorHelper EditorHelper { get { return _editorHelper; } }

    public static void SetEditorHelper(MissionUSEditorHelper helper)
    {
        _editorHelper = helper;
    }

    public static GUISkin GuiSkin { get; private set; }
    public static Texture2D Background { get; private set; }
    public static Texture2D BackgroundDark { get; private set; }
    public static Texture2D SecondaryBar { get; private set; }
    public static Texture2D Mask { get; private set; }
    public static Texture2D Knob { get; private set; }
    public static Texture2D HandleLine { get; private set; }
    public static Texture2D HandleLineGreen { get; private set; }
    public static Texture2D HandleLineGray { get; private set; }
    public static Texture2D Arrow { get; private set; }
    public static Texture2D ArrowGray { get; private set; }
    public static Texture2D ArrowGreen { get; private set; }
    public static Texture2D ArrowDownGray { get; private set; }
    public static Texture2D ArrowDownGreen { get; private set; }
    public static Texture2D ArrowLeftGray { get; private set; }
    public static Texture2D ArrowLeftGreen { get; private set; }
    private static Dictionary<string, GUIStyle> _customStyles = new Dictionary<string, GUIStyle>();
    private static Dictionary<string, GUIStyle> _origStyles = new Dictionary<string, GUIStyle>();
    private static Dictionary<string, GUIStyle> _appliedStyles = new Dictionary<string, GUIStyle>();

    public static GUIStyle GetStyle(string name) { return _customStyles[name.ToUpper()]; }



    public static string SerializeObject<T>(T obj)
    {
        var sb = new System.Text.StringBuilder();
        var writer = new JsonWriter(sb);
        writer.TypeHinting = true;
        JsonMapper.ToJson(obj, writer);
        return sb.ToString();
    }

    public static T DeserializeObject<T>(string data)
    {
        var reader = new JsonReader(data);
        reader.TypeHinting = true;
        return JsonMapper.ToObject<T>(reader);
    }

    static void Refresh()
    {
        var origStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        _origStyles = new Dictionary<string, GUIStyle>();
        for (int i = 0; i < origStyle.customStyles.Length; i++)
        {
            var style = origStyle.customStyles[i];
            if (style == null || style.name == null)
                continue;

            _origStyles[style.name.ToUpper()] = style;
        }

        _origStyles[GuiSkin.textArea.name.ToUpper()] = origStyle.textArea;
        _origStyles[GuiSkin.textField.name.ToUpper()] = origStyle.textField;
        _origStyles[GuiSkin.toggle.name.ToUpper()] = origStyle.toggle;
        _origStyles[GuiSkin.label.name.ToUpper()] = origStyle.label;
        _origStyles[EditorStyles.boldLabel.name.ToUpper()] = origStyle.label;

        _customStyles = new Dictionary<string, GUIStyle>();
        for (int i = 0; i < GuiSkin.customStyles.Length; i++)
        {
            var style = GuiSkin.customStyles[i];
            if (style == null || style.name == null)
                continue;

            _customStyles[style.name.ToUpper()] = style;
        }

        _customStyles[GuiSkin.textArea.name.ToUpper()] = GuiSkin.textArea;
        _customStyles[GuiSkin.textField.name.ToUpper()] = GuiSkin.textField;
        _customStyles[GuiSkin.toggle.name.ToUpper()] = GuiSkin.toggle;
        _customStyles[GuiSkin.label.name.ToUpper()] = GuiSkin.label;
        _customStyles[EditorStyles.boldLabel.name.ToUpper()] = GuiSkin.label;

        _appliedStyles = new Dictionary<string, GUIStyle>();
        _appliedStyles[EditorStyles.helpBox.name.ToUpper()] = EditorStyles.helpBox;
        _appliedStyles[EditorStyles.textArea.name.ToUpper()] = EditorStyles.textArea;
        _appliedStyles[EditorStyles.textField.name.ToUpper()] = EditorStyles.textField;
        _appliedStyles[EditorStyles.toggle.name.ToUpper()] = EditorStyles.toggle;
        _appliedStyles[EditorStyles.label.name.ToUpper()] = EditorStyles.label;
        _appliedStyles[EditorStyles.boldLabel.name.ToUpper()] = EditorStyles.boldLabel;
        _appliedStyles[EditorStyles.largeLabel.name.ToUpper()] = EditorStyles.largeLabel;
        _appliedStyles[EditorStyles.popup.name.ToUpper()] = EditorStyles.popup;
    }

    

    public static bool CenterButton(string text)
    {
        GUIContent btnTxt = null;
        btnTxt = new GUIContent(text);
        var rt = GUILayoutUtility.GetRect(btnTxt, GUI.skin.button, GUILayout.ExpandWidth(false));
        rt.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, rt.center.y);
        return GUI.Button(rt, text, GUI.skin.button);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        GuiSkin = EditorGUIUtility.Load("EngineEditorSkin.guiskin") as GUISkin;
        Background = EditorGUIUtility.Load("Textures/background.png") as Texture2D;
        BackgroundDark = EditorGUIUtility.Load("Textures/background_dark.png") as Texture2D;
        SecondaryBar = EditorGUIUtility.Load("Textures/secondary_bar.png") as Texture2D;
        Mask = EditorGUIUtility.Load("Textures/mask.png") as Texture2D;
        Knob = EditorGUIUtility.Load("Textures/knob.png") as Texture2D;
        HandleLine = EditorGUIUtility.Load("Textures/handle_line.png") as Texture2D;
        HandleLineGreen = EditorGUIUtility.Load("Textures/handle_line_green.png") as Texture2D;
        HandleLineGray = EditorGUIUtility.Load("Textures/handle_line_gray.png") as Texture2D;
        Arrow = EditorGUIUtility.Load("Textures/arrow.png") as Texture2D;
        ArrowGray = EditorGUIUtility.Load("Textures/arrow_gray.png") as Texture2D;
        ArrowGreen = EditorGUIUtility.Load("Textures/arrow_green.png") as Texture2D;
        ArrowDownGray = EditorGUIUtility.Load("Textures/arrow_gray_down.png") as Texture2D;
        ArrowDownGreen = EditorGUIUtility.Load("Textures/arrow_green_down.png") as Texture2D;
        ArrowLeftGray = EditorGUIUtility.Load("Textures/arrow_gray_left.png") as Texture2D;
        ArrowLeftGreen = EditorGUIUtility.Load("Textures/arrow_green_left.png") as Texture2D;
    }

    public static void ApplyCustomStyles()
    {
        Refresh();
        GUI.skin = MUSEditor.GuiSkin;
        foreach (var styleId in _appliedStyles.Keys)
        {
            Apply(_customStyles[styleId], _appliedStyles[styleId]);
        }
    }

    public static void ApplyOrigStyles()
    {
        Refresh();
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        foreach (var styleId in _appliedStyles.Keys)
        {
            Apply(_origStyles[styleId], _appliedStyles[styleId]);
        }
    }

    private static void Apply(GUIStyle to, GUIStyle from)
    {
        from.font = to.font;
        from.fontSize = to.fontSize;
        from.fontStyle = to.fontStyle;
        from.normal.textColor = to.normal.textColor;
        from.normal.background = to.normal.background;
        from.active.textColor = to.active.textColor;
        from.active.background = to.active.background;
        from.focused.textColor = to.focused.textColor;
        from.focused.background = to.focused.background;
        from.onNormal.background = to.onNormal.background;
        from.onActive.background = to.onActive.background;
        from.onFocused.background = to.onFocused.background;
        from.margin = to.margin;
        from.border = to.border;
        from.padding = to.padding;
        from.overflow = to.overflow;
        from.fixedWidth = to.fixedWidth;
        from.fixedHeight = to.fixedHeight;
        from.richText = to.richText;
    }

    static MUSEditor()
    {
        OnScriptsReloaded();
    }

}
