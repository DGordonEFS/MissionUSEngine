using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

using DarkChariotStudios.Dialogs;

public class ScriptEditorPage : EditorPage
{
    public override string Title
    {
        get
        {
            return Id;
        }
    }

    public string Id;
    private Script _script;
    public Script Script {
        get { return _script; }
        set
        {
            _script = value;
        }
    }

    public UnityEngine.Object Owner;
    
    public void Close()
    {
        ScriptEditor.GetInstance().Pages.Remove(this);
    }
}

public class ScriptEditor : MissionUSEditorWindow<ScriptEditorPage, Script>
{ 

    private static ScriptEditor _instance;
    public static ScriptEditor GetInstance()
    {
        if (_instance == null)
            Init();
        return _instance;
    }

    private float _sideBarWidth = 300;
   // public Script Script { get; set; }
    
    private Vector2 _scrollPos;
    
    private int _count;

    private const float COL_WIDTH = 200;

    [MenuItem("Mission US/Data/ScriptEditor")]
    static void Init()
    {
        if (_instance != null)
            return;

        var window = EditorWindow.GetWindow<ScriptEditor>();
        window.titleContent = new GUIContent("Script Editor");
        window.Show();

        OnScriptsReloaded();
        
        window.CanZoom = true;
        window.CanPan = true;
        window.TopBar = true;
        window.SecondaryBar = true;
        window.ExclusivePages = true;

        window.Mementos.OnChange = () =>
        {
        };
        window.Mementos.Restore = (value) => window.CurrentPage.Script = value;
        window.Mementos.GetMementoData = () => window.CurrentPage.Script;
        
        _instance = window;
    }

    protected override void OnDrawTopBar()
    {
        EditorGUILayout.BeginHorizontal();

        if (MUSEditor.EditorHelper.Button("Undo"))
            Mementos.Undo();
        else if (MUSEditor.EditorHelper.Button("Redo"))
            Mementos.Redo();


        EditorGUILayout.EndHorizontal();
    }

    protected override void OnDrawSecondaryBar()
    {
        DrawPageTabs();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        var typesWithMyAttribute =
            from a in System.AppDomain.CurrentDomain.GetAssemblies()
            from t in a.GetTypes()
            let attributes = t.GetCustomAttributes(typeof(ScriptActionData), true)
            where attributes != null && attributes.Length > 0
            select new { Type = t, Attributes = attributes.Cast<ScriptActionData>() };

       // Debug.Log("num types with attribute: " + typesWithMyAttribute.Count());
        Script.BlockTypes = new Dictionary<string, string>();
        Script.BlockIdLookup = new Dictionary<string, string>();
        Script.BlockGroupLookup = new Dictionary<string, string>();
        foreach (var type in typesWithMyAttribute)
        {
            foreach (var attribute in type.Attributes)
            {
                //Debug.Log("att: " + attribute.Id + ", type: " + type.Type);
                Script.BlockTypes.Add(attribute.Id, type.Type.Name);
                Script.BlockIdLookup.Add(type.Type.Name, attribute.Id);
                Script.BlockGroupLookup.Add(attribute.Id, attribute.Group);
            }
        }
        Script.BlockIds = Script.BlockTypes.Keys.ToList();
    }

    void OnDestroy()
    {
        _instance = null;
    }

    protected override void OnGUIDraw()
    {
        if (CurrentPage == null)
            return;

        _count = 0;

        BeginZoom(Zoom, new Rect(0, 0, Screen.width, Screen.height));
        BeginWindows();
        GUILayout.Space(100);
        Debug.Log("Current Script: " + CurrentPage.Script.Blocks.Count);
        DrawColumn(CurrentPage.Script.Blocks, 0, 100);
        EndWindows();
        EndZoom();
    }

    float DrawColumn(List<ScriptBlock> blocks, int col, float y)
    {
        float x = 50 * (col+1) + 400 * col;
        float nextX = 50 * (col + 2) + 400 * (col + 1);

        float buffer = 75;

        var idToIndex = new Dictionary<int, int>();
        
        UnityEngine.GUI.WindowFunction drawBlock = (int id) =>
        {
            var index = idToIndex[id];
            ScriptBlock block = blocks[index];
            block.OwnerBlocks = blocks;
            block.CreateMemento = () => Mementos.CreateMemento();
            block.OnGUI(index > 0 ? blocks[index - 1] : null);
        };

        for (int i = 0; i < blocks.Count; i++)
        {
            ScriptBlock block = blocks[i];

            if (block.IsDisposed)
            {
                blocks.Remove(block);
                i--;
                continue;
            }

            block.OwnerBlocks = blocks;

            ScriptBlock nextBlock = null;

            if (i < blocks.Count - 1)
                nextBlock = blocks[i + 1];

            _count++;
            idToIndex[_count] = i;

            var extend = 0;
            var blockRect = GUILayout.Window(_count, new Rect(x + Pan.x, y + Pan.y, block.EditorSize.x, block.EditorSize.y + extend), drawBlock, "");
            block.EditorSize.x = blockRect.width;
            block.EditorSize.y = blockRect.height - extend;
            
            y += blockRect.height + buffer;
            var oldY = y;

            Vector2 startPos = default(Vector2);
            Vector2 endPos = default(Vector2);
            Color color = default(Color);
            Texture2D arrow = null;
            var arrowSize = 20 * Zoom;

            var container = block as ScriptContainer;
            if (container != null && !container.Folded)
            {
                color = Color.gray;
                arrow = MUSEditor.ArrowDownGray;
                if (((ScriptContainer)block).Entered)
                {
                    color = Color.green;
                    arrow = MUSEditor.ArrowDownGray;
                }

                startPos = new Vector2(blockRect.x + blockRect.width, blockRect.y + blockRect.height / 2);
                endPos = new Vector2(Pan.x + nextX + blockRect.width/2, blockRect.y + blockRect.height / 2);
                Handles.DrawBezier(startPos, endPos, startPos, endPos, color, MUSEditor.HandleLine, 3);

                startPos = new Vector2(endPos.x, endPos.y);
                endPos = new Vector2(endPos.x, y + Pan.y - 30);
                Handles.DrawBezier(startPos, endPos, startPos, endPos, color, MUSEditor.HandleLine, 3);

                GUI.DrawTexture(new Rect(endPos.x - arrowSize / 2 - 0.5f, endPos.y - arrowSize / 2 - 0.5f, arrowSize, arrowSize), arrow);

                var count = _count;
                y = DrawColumn(((ScriptContainer)block).Contents.Then, col + 1, y);

                if (count != _count && i != blocks.Count-1)
                {
                    color = Color.gray;
                    arrow = MUSEditor.ArrowLeftGray;
                    if (((ScriptContainer)block).Entered)
                    {
                        color = Color.green;
                        arrow = MUSEditor.ArrowLeftGray;
                    }

                    startPos = new Vector2(endPos.x, Pan.y + y + 20);
                    endPos = new Vector2(endPos.x, startPos.y + 15);
                    Handles.DrawBezier(startPos, endPos, startPos, endPos, color, MUSEditor.HandleLine, 3);

                    startPos = new Vector2(endPos.x, endPos.y);
                    endPos = new Vector2(endPos.x - 200, endPos.y);
                    Handles.DrawBezier(startPos, endPos, startPos, endPos, color, MUSEditor.HandleLine, 3);

                    GUI.DrawTexture(new Rect(endPos.x - arrowSize / 2 - 0.5f, endPos.y - arrowSize / 2 - 0.5f, arrowSize, arrowSize), arrow);
                }
            }
            
            color = Color.gray;
            arrow = MUSEditor.ArrowDownGray;
            if (block.Used && nextBlock != null && nextBlock.Used)
            {
                color = Color.green;
                arrow = MUSEditor.ArrowDownGreen;
            }

            startPos = new Vector2(blockRect.x + blockRect.width / 2, Pan.y + oldY - buffer + 15);
            endPos = new Vector2(blockRect.x + blockRect.width / 2, Pan.y + y - 25);

            Handles.DrawBezier(startPos, endPos, startPos, endPos, color, MUSEditor.HandleLine, 3);
            
            GUI.DrawTexture(new Rect(endPos.x - arrowSize / 2 - 0.5f, endPos.y - arrowSize / 2 - 0.5f, arrowSize, arrowSize), arrow);
        }

        
        GUILayout.BeginArea(new Rect(x + Pan.x + 160, y + Pan.y - 10, COL_WIDTH, 100));
        GUILayout.Space(10);
        if (MUSEditor.EditorHelper.Button("Add Block"))
        {
            var memento = new Memento<Script>(CurrentPage.Script);
            Mementos.CreateMemento();
            var block = new ScriptWaitAction();
            blocks.Add(block);
        }
        GUILayout.EndArea();
        
        return y;
    }
    
}
