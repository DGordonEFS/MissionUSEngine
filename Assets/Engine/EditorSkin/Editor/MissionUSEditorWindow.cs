using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class MissionUSEditorWindow : EditorWindow
{
    public bool CanZoom = false;
    public bool CanPan = false;
    public float Zoom = 1;

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
        GUI.BeginGroup(new Rect(0, 21, Screen.width, Screen.height));
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
        

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), InitEditor.Background);

        InitEditor.ApplyCustomStyles();
        using (new EditorGUI.DisabledScope(!Enable))
        {
            OnGUIDraw();
        }

        InitEditor.ApplyOrigStyles();

        if (!Enable)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), InitEditor.Mask);
        }

        if (forceRepaint)
        {
            Repaint();
        }
    }

    protected void DrawToolbar(Rect rect)
    {
        GUI.DrawTexture(rect, InitEditor.BackgroundDark);
    }

    protected void DrawMask()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), InitEditor.Mask);
    }

    protected virtual void OnGUIDraw()
    {

    }
}
