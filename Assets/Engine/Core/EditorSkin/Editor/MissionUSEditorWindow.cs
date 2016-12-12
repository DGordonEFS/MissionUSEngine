using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class EditorPage
{
    public string Group;
    public virtual string Title { get { return null; } }
}

public class MissionUSEditorWindow<TPage> : EditorWindow where TPage : EditorPage
{
    private MissionUSEditorHelper _editorHelper = new MissionUSEditorHelper();

    public float TopBarHeight { get { return 30; } }
    public float SecondaryBarHeight { get { return 30; } }
    public bool CanZoom = false;
    public bool CanPan = false;
    public float Zoom = 1;
    public bool TopBar = false;
    public bool SecondaryBar = false;
    public bool MainArea = false;

    public bool ExclusivePages;
    public string PageGroup;
    public int CurrentPageIndex { get; set; }
    public List<TPage> Pages { get; set; }
    public TPage CurrentPage { get { return Pages != null && CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count ? Pages[CurrentPageIndex] : null; } }

    public void AddPage(TPage page)
    {
        if (ExclusivePages)
            Pages.Clear();
        Pages.Add(page);
        CurrentPageIndex = Pages.Count - 1;
        Repaint();
    }

    public MissionUSEditorWindow()
    {
        Pages = new List<TPage>();
        Repaint();
    }

    protected void DrawPageTabs()
    {
        if (CurrentPage == null)
            return;

        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < Pages.Count; i++)
        {
            var page = Pages[i];

            if (CurrentPage == page)
            {
                GUIStyle style = "selectedtab";
                if (MUSEditor.EditorHelper.Button(Pages[i].Title, style))
                {
                    CurrentPageIndex = i;
                }
            }
            else
            {
                GUIStyle style = "tab";
                if (MUSEditor.EditorHelper.Button(Pages[i].Title, style))
                {
                    CurrentPageIndex = i;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public void ResetPan() { _pan = Vector2.zero; }

    private Vector2 _pan = new Vector2();

    public Vector2 Pan { get { return new Vector2(_pan.x, _pan.y); } }

    public Rect GetCenterRect(GUIContent content, GUIStyle style)
    {
        var rt = GUILayoutUtility.GetRect(content, style, GUILayout.ExpandWidth(false));
        rt.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, rt.center.y);
        return rt;
    }

    public void CenterLabel(string text, GUIStyle style = null)
    {
        if (style == null)
            style = GUI.skin.label;
        GUI.Label(GetCenterRect(new GUIContent(text), style), text, style);
    }

    private static Stack<Matrix4x4> previousMatrices = new Stack<Matrix4x4>();

    public static Rect BeginZoom(float zoomScale, Rect screenCoordsArea)
    {
        GUI.EndGroup();

        Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.center);
        clippedArea.y += 21;

        GUI.BeginGroup(clippedArea);

        previousMatrices.Push(GUI.matrix);
        Matrix4x4 translation = Matrix4x4.TRS(clippedArea.center, Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
        GUI.matrix = translation * scale * translation.inverse;

        return clippedArea;
    }

    /// <summary>
    /// Ends the zoom area
    /// </summary>
    public static void EndZoom()
    {
        GUI.matrix = previousMatrices.Pop();
        GUI.EndGroup();
        GUI.BeginGroup(new Rect(0, 19, Screen.width, Screen.height));
    }

    private bool _enable = true;
    public bool Enable
    {
        get { return _enable; }
        set
        {
            _enable = value;
        }
    }

    void DrawTopBar()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, TopBarHeight), MUSEditor.BackgroundDark);
        GUILayout.BeginArea(new Rect(5, 0, Screen.width, TopBarHeight));
        OnDrawTopBar();
        GUILayout.EndArea();
    }

    protected virtual void OnDrawTopBar()
    {

    }

    void DrawSecondaryBar()
    {
        GUI.DrawTexture(new Rect(0, TopBarHeight, Screen.width, SecondaryBarHeight), MUSEditor.SecondaryBar);
        GUILayout.BeginArea(new Rect(5, TopBarHeight, Screen.width, SecondaryBarHeight));
        OnDrawSecondaryBar();
        GUILayout.EndArea();
    }

    protected virtual void OnDrawSecondaryBar()
    {

    }

    void DrawMainArea()
    {
        float barHeight = 0;

        if (TopBar)
            barHeight += TopBarHeight;
        if (TopBar && SecondaryBar)
            barHeight += SecondaryBarHeight;

        BeginZoom(Zoom, new Rect(0, barHeight - 2 - (Mathf.Max(0, Zoom - 1) * barHeight * 0.4f), Screen.width, Screen.height - barHeight));
        BeginWindows();
        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height - barHeight));
        OnDrawMainArea();
        GUILayout.EndArea();
        EndWindows();
        EndZoom();
    }

    protected virtual void OnDrawMainArea()
    {

    }
    
    void OnGUI()
    {
        bool forceRepaint = false;
        if (Enable)
        {
            if (CanZoom && Event.current.type == EventType.ScrollWheel)
            {
                Zoom += Event.current.delta.y * 0.01f;
                Zoom = Mathf.Clamp(Zoom, 0.5f, 2);
                forceRepaint = true;
            }


            if (CanPan && Event.current.button == 2 && Event.current.type == EventType.MouseDrag)
            {
                _pan.x += Event.current.delta.x;
                _pan.y += Event.current.delta.y;
                forceRepaint = true;
            }
        }

        _editorHelper.Begin(Zoom);
        MUSEditor.SetEditorHelper(_editorHelper);

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MUSEditor.Background);

        MUSEditor.ApplyCustomStyles();
        using (new EditorGUI.DisabledScope(!Enable))
        {

            OnGUIDraw();
            if (MainArea)
                DrawMainArea();
            if (TopBar)
                DrawTopBar();
            if (TopBar && SecondaryBar)
                DrawSecondaryBar();
        }

        _editorHelper.End();

        MUSEditor.ApplyOrigStyles();


        if (!Enable)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MUSEditor.Mask);
        }
        

        if (forceRepaint)
        {
            Repaint();
        }
    }

    protected void DrawToolbar(Rect rect)
    {
        GUI.DrawTexture(rect, MUSEditor.BackgroundDark);
    }

    protected void DrawMask()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MUSEditor.Mask);
    }

    protected virtual void OnGUIDraw()
    {

    }
}
