using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using System;
using System.IO;

using DarkChariotStudios.Dialogs;
using LitJson;

using System.Linq;

public class DrawHandleData
{
    public Vector2 from;
    public int fromDir = 1;
    public Vector2 to;
    public int toDir = -1;
}

public class ConnectionData
{
    public string From;
    public string To;
}

public class DialogEditorPage
{
    public Dialog Dialog = new Dialog() { Id = "Untitled" };
    public List<DialogNode> Nodes;
}

public class DialogEditor : MissionUSEditorWindow
{
    public static DialogEditor Instance { get; private set; }

    public const string PATH = "Assets/Engine/Data/Dialogs/Resources/";
    
    private Vector2 _scrollPos;
    public Dialog Dialog { get { return CurrentPage != null ? CurrentPage.Dialog : null; } }

    private List<DialogNode> Nodes { get { return CurrentPage != null ? CurrentPage.Nodes : null; } set { if (CurrentPage == null) return; CurrentPage.Nodes = value; } }

    public int CurrentPageIndex { get; private set; }
    public List <DialogEditorPage> Pages { get; private set; }
    public DialogEditorPage CurrentPage { get { return Pages != null && CurrentPageIndex >= 0 && CurrentPageIndex < Pages.Count ? Pages[CurrentPageIndex] : null; } }



    private bool _saveAs;
    private string _saveAsId;

    private Dictionary<DialogResponse, DrawHandleData> _handles = new Dictionary<DialogResponse, DrawHandleData>();

    [MenuItem("Mission US/DialogEditor")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<DialogEditor>();
        window.titleContent = new GUIContent("Dialog Editor");
        window.Show();

        Instance = window;
        window.CanZoom = true;
        window.CanPan = true;
        window.TopBar = true;
        window.SecondaryBar = true;
        window.MainArea = true;
        window.Pages = new List<DialogEditorPage>();
        window.Pages.Add(new DialogEditorPage());
    }

    void OnDestroy()
    {
        Instance = null;
    }

    private void RefreshConnections()
    {
        var connections = new List<ConnectionData>();
        for (int i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];
            if (node.From == null)
                node.From = new Dictionary<string, float>();
        }

        for (int i = 0; i < Nodes.Count; i++)
        {
            var node = Nodes[i];

            var removeFromList = new List<string>();
            foreach (var fromId in node.From.Keys)
            {
                if (!Dialog.ContainsNode(fromId))
                {
                    removeFromList.Add(fromId);
                    continue;
                }

                var fromNode = Dialog.GetNode(fromId);

                bool found = false;
                var fromResponses = fromNode.GetResponses();
                for (int j = 0; j < fromResponses.Length; j++)
                {
                    var response = fromResponses[j];

                    if (response.NextNodeId == node.Id)
                        found = true;
                }
                if (!found)
                {
                    //Debug.Log("didnt find from: " + fromId + " for node: " + node.Id);
                    removeFromList.Add(fromId);
                    continue;
                }

                if (connections.Find((x) => (x.From == node.Id && x.To == fromId) || (x.From == fromId && x.To == node.Id)) == null)
                    connections.Add(new ConnectionData() { From = node.Id, To = fromId });
            }

            for (int j = 0; j < removeFromList.Count; j++)
                node.From.Remove(removeFromList[j]);

            var responses = node.GetResponses();
            for (int j = 0; j < responses.Length; j++)
            {
                var response = responses[j];
                if (!Dialog.ContainsNode(response.NextNodeId))
                    continue;

                var nextNode = Dialog.GetNode(response.NextNodeId);

                if (nextNode.Id == node.Id)
                    continue;

                if (nextNode.From != null && !nextNode.From.ContainsKey(node.Id))
                    nextNode.From.Add(node.Id, 0);
            }
        }



        for (int i = 0; i < connections.Count; i++)
        {
            var connection = connections[i];
            var from = Dialog.GetNode(connection.From);
            var to = Dialog.GetNode(connection.To);

            var startPos = new Vector2(from.EditorPosition.x + from.EditorPosition.width / 2, from.EditorPosition.y + from.EditorPosition.height / 2) + Pan;// Vector2.zero;
            var endPos = new Vector2(to.EditorPosition.x + to.EditorPosition.width / 2, to.EditorPosition.y + to.EditorPosition.height / 2) + Pan;//Vector2.zero;

            startPos *= Zoom;
            endPos *= Zoom;

            var handleLine = MUSEditor.HandleLineGray;

            var color = Color.gray;
            var arrow = MUSEditor.ArrowGray;
            if (from.Used && to.Used)
            {
                color = Color.green;
                arrow = MUSEditor.ArrowGreen;
            }

            Handles.DrawBezier(startPos, endPos, startPos, endPos, color, MUSEditor.HandleLine, 3);

            var diff = endPos - startPos;
            var angleBetween = Mathf.Atan2(diff.y, diff.x) * 180 / Mathf.PI + 180;

            var centerPos = startPos + (diff / 2);

            var arrowSize = 30 * Zoom;
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(angleBetween, centerPos);
            GUI.DrawTexture(new Rect(centerPos.x - arrowSize / 2, centerPos.y - arrowSize / 2, arrowSize, arrowSize), arrow);
            GUI.matrix = matrixBackup;
        }
    }
    
    protected override void OnGUIDraw()
    {
        if (CurrentPage == null)
            return;

        if (Dialog != null)
            Nodes = Dialog.GetNodes();
        else
            Nodes = new List<DialogNode>();
        
        RefreshConnections();
    }
    
    protected override void OnDrawTopBar()
    {
        using (new EditorGUI.DisabledScope(_saveAs))
        {
            DrawTopBarDialogButtons();
            DrawTopBarNodeButtons();
        }
    }

    protected override void OnDrawMainArea()
    {
        if (CurrentPage == null)
            return;

        using (new EditorGUI.DisabledScope(_saveAs))
        {
            using (new GUILayout.AreaScope(new Rect(100, 100, Screen.width / 2, Screen.height / 2)))
            {
                BeginZoom(Zoom, new Rect(0, 0, Screen.width, Screen.height));
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var node = Nodes[i];
                    var pan = Pan;
                    node.EditorPosition = GUILayout.Window(i, new Rect(node.EditorPosition.x + Pan.x, node.EditorPosition.y + Pan.y, node.EditorPosition.width, node.EditorPosition.height), DrawNode, "");
                    node.EditorPosition.x -= pan.x;
                    node.EditorPosition.y -= pan.y;
                }
                EndZoom();
            }
        }
    }

    void DrawTopBarDialogButtons()
    {
        GUILayout.BeginArea(new Rect(0, 0, 400, Screen.height));
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New Dialog"))
        {
            ResetPan();
            Zoom = 1;

            var page = new DialogEditorPage();
            Pages.Add(page);
            page.Dialog = new Dialog();
            page.Dialog.Id = "test";
            var node = page.Dialog.CreateNode(page.Dialog.GetUniqueId());
            node.AddPrompt("");
            var response = node.AddResponse("");
            response.NextNodeType = DialogResponse.NextNodeTypes.End;
            page.Nodes = page.Dialog.GetNodes();
            CurrentPageIndex = Pages.Count - 1;
        }

        if (GUILayout.Button("Load Dialog"))
        {
            ResetPan();
            Zoom = 1;
            Load("test");
        }

        if (GUILayout.Button("Save Dialog"))
        {
            if (Dialog.Id == null)
                SaveAs();
            else
                Save(GetPath());
        }

        if (GUILayout.Button("Save Dialog As"))
        {
            SaveAs();
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    protected override void OnDrawSecondaryBar()
    {
        if (Pages == null)
            return;

        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < Pages.Count; i++)
        {
            var page = Pages[i];

            if (CurrentPage == page)
            {
                GUIStyle style = "selectedtab";
                if (GUILayout.Button(page.Dialog.Id, style))
                {
                    CurrentPageIndex = i;
                }
            }
            else
            {
                GUIStyle style = "tab";
                if (GUILayout.Button(page.Dialog.Id, style))
                {
                    CurrentPageIndex = i;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public string GetPath()
    {
        return PATH + Dialog.Id + ".txt";
    }

    public bool DoesPathExist(string path = null)
    {
        if (path == null)
            path = GetPath();
        
        return File.Exists(path);
    }

    public void Load(string dialogId)
    {
        var text = Resources.Load<TextAsset>(dialogId).text;
        var page = new DialogEditorPage();
        page.Dialog = JsonMapper.ToObject<Dialog>(text);
        Pages.Add(page);
        CurrentPageIndex = Pages.Count - 1;
    }

    public void Save(string path)
    {
        var data = JsonMapper.ToJson(Dialog);
        Debug.Log("save to: " + path);
        var sr = File.CreateText(path);
        sr.Write(data);
        sr.Close();
        AssetDatabase.Refresh();
    }

    public void SaveAs()
    {
        DialogSaveAsPopup.Init(this);
    }

    void DrawTopBarNodeButtons()
    {
        GUILayout.BeginArea(new Rect(500, 0, 400, Screen.height));
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New Node"))
        {
            var node = Dialog.CreateNode(Dialog.GetUniqueId());
            node.AddPrompt("");
            var response = node.AddResponse("");
            response.NextNodeType = DialogResponse.NextNodeTypes.End;
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void DrawNode(int nodeIndex)
    {
        using (new EditorGUI.DisabledScope(!Enable))
        {
            var node = Nodes[nodeIndex];

            GUI.Label(new Rect(node.EditorPosition.width / 2 - 25, 15, node.EditorPosition.width, TopBarHeight), node.Id, EditorStyles.largeLabel);
            if (GUI.Button(new Rect(node.EditorPosition.width - 45, 25, 25, 25), "X"))
            {
                Dialog.RemoveNode(node.Id);
            }

            //  GUI.DrawTexture(new Rect(0, 25, 15, 15), InitEditor.Knob);
            //  GUI.DrawTexture(new Rect(node.EditorPosition.width - 15, 25, 15, 15), InitEditor.Knob);

            //   GUI.DrawTexture(new Rect(0, node.EditorPosition.height - 35, 15, 15), InitEditor.Knob);
            //  GUI.DrawTexture(new Rect(node.EditorPosition.width - 15, node.EditorPosition.height - 35, 15, 15), InitEditor.Knob);

            GUILayout.Space(30);


            if (GUILayout.Button("P+"))
            {
                node.AddPrompt("", "");
            }
            var prompts = node.GetPrompts();
            for (int i = 0; i < prompts.Length; i++)
            {
                var prompt = prompts[i];

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("P" + i, GUILayout.Width(30));
                    prompt.Text = EditorGUILayout.TextArea(prompt.Text);
                    List<string> npcs = new List<string>() { "Bob", "Mary", "Joe" };
                    int indexOfId = 0;
                    if (npcs.Contains(prompt.Npc))
                        indexOfId = npcs.IndexOf(prompt.Npc);
                    prompt.Npc = npcs[EditorGUILayout.Popup(indexOfId, npcs.ToArray(), GUILayout.Width(50))];
                    if (GUILayout.Button("A"))
                    {
                    }
                    if (GUILayout.Button("C"))
                    {
                    }
                    if (GUILayout.Button("X"))
                    {
                        node.RemovePrompt(prompt);
                    }
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();


            if (GUILayout.Button("R+"))
            {
                node.AddResponse("", "");
            }
            var toIds = new List<string>();
            var responses = node.GetResponses();
            for (int i = 0; i < responses.Length; i++)
            {
                var response = responses[i];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField("R" + i, GUILayout.Width(30));
                response.Text = EditorGUILayout.TextArea(response.Text);
                response.NextNodeType = (DialogResponse.NextNodeTypes)EditorGUILayout.EnumPopup(response.NextNodeType, GUILayout.Width(50));

                switch (response.NextNodeType)
                {
                    case DialogResponse.NextNodeTypes.Id:
                        var nodeIds = Dialog.GetNodes().ConvertAll<string>((x) => x.Id);
                        int indexOfId = 0;
                        if (nodeIds.Contains(response.NextNodeId))
                            indexOfId = nodeIds.IndexOf(response.NextNodeId);

                        var newNextNodeId = nodeIds[EditorGUILayout.Popup(indexOfId, nodeIds.ToArray(), GUILayout.Width(50))];
                        response.NextNodeId = newNextNodeId;
                        if (!toIds.Contains(newNextNodeId))
                            toIds.Add(newNextNodeId);
                        break;
                    case DialogResponse.NextNodeTypes.End:
                        response.NextNodeId = null;
                        break;
                    case DialogResponse.NextNodeTypes.Script:
                        response.NextNodeId = null;
                        if (GUILayout.Button("N"))
                        {
                        }
                        break;
                }
                if (GUILayout.Button("D"))
                {
                }
                if (GUILayout.Button("A"))
                {
                }
                if (GUILayout.Button("C"))
                {
                }
                if (GUILayout.Button("X"))
                {
                    node.RemoveResponse(response);
                }
                GUILayout.Space(15);
                /*
                    var spaceRect = GUILayoutUtility.GetLastRect();
                    //GUI.DrawTexture(new Rect(node.EditorPosition.width - 15, spaceRect.y+2, 15, 15), InitEditor.Knob);
                    //GUI.DrawTexture(new Rect(0, spaceRect.y+2, 15, 15), InitEditor.Knob);



                    if (!spaceRect.Equals(new Rect(0, 0, 1, 1)))
                    {
                        if (response.NextNodeType == DialogResponse.NextNodeTypes.Id && response.NextNodeId != node.Id)
                        {
                            var nextNode = Dialog.GetNode(response.NextNodeId);

                            var nextNodePosition = nextNode.EditorPosition;
                            var from = new Vector2(node.EditorPosition.x + spaceRect.x + 25f, node.EditorPosition.y + spaceRect.y + 7.5f);
                            var to = new Vector2(nextNodePosition.x + 7.5f, nextNodePosition.y + 25 + 7.5f);
                            var fromDir = 1;
                            var toDir = -1;

                            if (nextNode.From != null && nextNode.From.ContainsKey(node.Id))
                            {
                                to.y += nextNode.From[node.Id];
                            }

                            if (nextNodePosition.x < node.EditorPosition.x)
                            {
                                from.x = node.EditorPosition.x;
                                fromDir = -1;
                                to.x = nextNodePosition.x + nextNodePosition.width;
                                toDir = 1;
                            }

                            response.EditorDrawLine = true;
                            var drawData = new DrawHandleData() { from = from, to = to, fromDir = fromDir, toDir = toDir };

                            if (!_handles.ContainsKey(response))
                                _handles.Add(response, drawData);
                            else
                                _handles[response] = drawData;

                        }
                        else
                        {
                            response.EditorDrawLine = false;
                        }
                    }
                        */
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(30);
            if (toIds.Count > 0)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("To:", GUILayout.Width(40));
                EditorGUILayout.LabelField(string.Join(", ", toIds.ToArray()));
                EditorGUILayout.EndHorizontal();
            }

            var fromIds = node.From.Keys.ToList();
            if (fromIds.Count > 0)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("From:", GUILayout.Width(40));
                EditorGUILayout.LabelField(string.Join(", ", fromIds.ToArray()));
                EditorGUILayout.EndHorizontal();
            }

            if (toIds.Count > 0 || fromIds.Count > 0)
                GUILayout.Space(20);

            GUI.DragWindow();
        }
    }

    void DrawSaveAs(bool saveAs)
    {
        if (!saveAs)
            return;

        DrawMask();


        GUILayout.BeginArea(new Rect(0, TopBarHeight, Screen.width, Screen.height));
        
        GUI.Window(200, new Rect(Screen.width/2 - 300, Screen.height/2 - 150, 600, 300), DrawSaveAsWindow, "");
        
        GUILayout.EndArea();
    }

    void DrawSaveAsWindow(int id)
    {
        using (new GUILayout.AreaScope(new Rect(50, 50, 500, 200)))
        {
            _saveAsId = EditorGUILayout.TextField("Save Dialog As:", _saveAsId);

            if (DoesPathExist(_saveAsId))
            {
                EditorGUILayout.HelpBox("A dialog with this name already exists. Save to overwrite the existing dialog.", MessageType.Warning);
            }
        }
        
        GUILayout.BeginArea(new Rect(425, 250, 600, 50));
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            _saveAs = false;
        }

        if (GUILayout.Button("Save As"))
        {
            Dialog.Id = _saveAsId;
            Save(GetPath());
            _saveAs = false;
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
            
    }
    
}

public class DialogSaveAsPopup : MissionUSEditorWindow
{
    public static DialogSaveAsPopup Init(DialogEditor launcher)
    {
        var window = new DialogSaveAsPopup();
        window.ShowPopup();
        window._owner = launcher;
        window._saveAsId = window._owner.Dialog.Id;
        window._owner.Enable = false;
        Debug.Log("new dialog popup");
        return window;
    }

    private DialogEditor _owner;
    private string _saveAsId;

    protected override void OnGUIDraw()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), MUSEditor.Background);
        var size = new Vector2(600, 200);
        position = new Rect(_owner.position.x + _owner.position.width/2 - size.x/2, _owner.position.y + _owner.position.height/2 - size.y/2, size.x, size.y);


        using (new GUILayout.AreaScope(new Rect(50, 50, 500, 150)))
        {
            _saveAsId = EditorGUILayout.TextField("Save Dialog As:", _saveAsId);

            if (_owner.DoesPathExist(DialogEditor.PATH + _saveAsId + ".txt"))
            {
                EditorGUILayout.HelpBox("A dialog with this name already exists. Save to overwrite the existing dialog.", MessageType.Warning);
            }
        }

        GUILayout.BeginArea(new Rect(425, 160, 600, 50));
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cancel"))
        {
            _owner.Enable = true;
            _owner.Repaint();
            Close();
        }

        if (GUILayout.Button("Save As"))
        {
            _owner.Enable = true;
            _owner.Dialog.Id = _saveAsId;
            _owner.Save(_owner.GetPath());
            _owner.Repaint();
             Close();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void OnDestroy()
    {
        _owner.Enable = true;
    }

    void OnLostFocus()
    {
        _owner.Enable = true;
    }
}