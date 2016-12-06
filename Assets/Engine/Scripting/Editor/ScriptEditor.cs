using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

using DarkChariotStudios.Dialogs;

public class ScriptEditor : EditorWindow
{

    public static ScriptEditor Instance { get; private set; }

    private float _sideBarWidth = 300;
    public Script Script { get; set; }

    private List<DialogNode> _nodes;

    private Vector2 _scrollPos;

    private float _totalHeight = 0;

    private const float COL_WIDTH = 200;

    [MenuItem("Mission US/ScriptEditor")]
    static void Init()
    {
        if (Instance != null)
            return;

        var window = EditorWindow.GetWindow<ScriptEditor>();
        window.titleContent = new GUIContent("Script Editor");
        window.Show();

        var typesWithMyAttribute =
            from a in System.AppDomain.CurrentDomain.GetAssemblies()
            from t in a.GetTypes()
            let attributes = t.GetCustomAttributes(typeof(ScriptActionData), true)
            where attributes != null && attributes.Length > 0
            select new { Type = t, Attributes = attributes.Cast<ScriptActionData>() };
        
        Script.BlockTypes = new Dictionary<string, Type>();
        Script.BlockIdLookup = new Dictionary<Type, string>();
        foreach (var type in typesWithMyAttribute)
        {
            foreach (var attribute in type.Attributes)
            {
                Script.BlockTypes.Add(attribute.Id, type.Type);
                Script.BlockIdLookup.Add(type.Type, attribute.Id);
            }
        }
        Script.BlockIds = Script.BlockTypes.Keys.ToList();

        window.Script = new Script();
        Instance = window;
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void OnGUI()
    {
        if (Script == null)
            return;

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
        
        _totalHeight = DrawScriptBlocks(Script.Blocks, 50, 0);
        EditorGUILayout.EndScrollView();
    }

    float DrawScriptBlocks(List<ScriptBlock> scriptBlocks, float totalHeight, int col)
    {
        for (int i = 0; i < scriptBlocks.Count; i++)
        {
            ScriptBlock block = scriptBlocks[i];
            block.OwnerBlocks = scriptBlocks;

            var tex = new Texture2D((int)COL_WIDTH, 50);
            for (int x = 0; x < COL_WIDTH; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    tex.SetPixel(x, y, Color.black);
                }
            }
            tex.Apply();

            GUILayout.BeginArea(new Rect(50 + col * (COL_WIDTH + 25), totalHeight, COL_WIDTH, 50), tex);
            totalHeight += block.OnGUI();
            GUILayout.EndArea();

            if (block is ScriptIfThenContainer)
            {
                var ifThen = (ScriptIfThenContainer)block;
                totalHeight = DrawScriptBlocks(ifThen.If.Then, totalHeight, col + 1);

                for (int j = 0; j < ifThen.ElseIf.Count; j++)
                {
                    var elseThen = ifThen.ElseIf[j];

                    GUILayout.BeginArea(new Rect(50 + col * (COL_WIDTH + 25), totalHeight, COL_WIDTH, 50), tex);
                    totalHeight += elseThen.OnGUI();
                    GUILayout.EndArea();

                    totalHeight = DrawScriptBlocks(elseThen.Then, totalHeight, col + 1);
                }

                if (ifThen.Else != null)
                {
                    GUILayout.BeginArea(new Rect(50 + col * (COL_WIDTH + 25), totalHeight, COL_WIDTH, 50), tex);
                    totalHeight += ifThen.Else.OnGUI();
                    GUILayout.EndArea();

                    totalHeight = DrawScriptBlocks(ifThen.Else.Then, totalHeight, col + 1);
                }
            }
            else if (block is ScriptWhileContainer)
            {
                var whileThen = (ScriptWhileContainer)block;
                totalHeight = DrawScriptBlocks(whileThen.While.Then, totalHeight, col + 1);
            }
        }
        
        GUILayout.BeginArea(new Rect(50 + col * (COL_WIDTH+25), totalHeight, COL_WIDTH, 50));
        if (GUILayout.Button("Add Block"))
            scriptBlocks.Add(new ScriptNoneAction());
        GUILayout.EndArea();

        return totalHeight;
    }
    
}
