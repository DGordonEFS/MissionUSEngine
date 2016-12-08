using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LitJson;

namespace DarkChariotStudios.Dialogs
{
    public class Dialog
    {
        public delegate void DialogNodeEventHandler(DialogNode node);
        [JsonIgnore]
        public DialogNodeEventHandler onDialogNodeChange;

        public int UniqueNodes;
        public string GetUniqueId() { return "n" + UniqueNodes++; }

        public string Id { get; set; }
        public string DefaultNodeId;

        
        [JsonInclude]
        private Dictionary<string, DialogNode> _nodes = new Dictionary<string, DialogNode>();

        [JsonIgnore]
        public int NumNodes { get { return _nodes.Count; } }

        [JsonIgnore]
        public bool IsClosed { get; private set; }

        [JsonIgnore]
        public DialogNode CurrentNode { get; private set; }

        public DialogNode CreateNode(string id)
        {
            DialogNode node = new DialogNode(this, id);
            _nodes[id.ToUpper()] = node;
            return node;
        }

        public void AddNode(DialogNode node)
        {
            _nodes[node.Id.ToUpper()] = node;
        }

        public void RemoveNode(string id)
        {
            _nodes.Remove(id.ToUpper());
        }

        public List<DialogNode> GetNodes()
        {
            return _nodes.Values.ToList();
        }

        public DialogNode GetNode(string id)
        {
            return _nodes[id.ToUpper()];
        }

        public bool ContainsNode(string id)
        {
            if (id == null)
                return false;
            return _nodes.ContainsKey(id.ToUpper());
        }

        public IEnumerator SetCurrentNode(string id)
        {
            Debug.Log("set current node: " + id);
            CurrentNode = GetNode(id);
            yield return CurrentNode.MakeCurrent();

            if (onDialogNodeChange != null)
                onDialogNodeChange(CurrentNode);
        }

        public IEnumerator SelectResponse(int index)
        {
            DialogResponse response = CurrentNode.CurrentResponses[index];
            
            yield return response.Execute();
            
            if (response.NextNodeId != null && response.NextNodeId != "")
                yield return SetCurrentNode(response.NextNodeId);
            else
                yield return Close();
        }

        public IEnumerator Close()
        {
            yield return OnClose();
            IsClosed = true;
        }

        public virtual IEnumerator OnLaunch(string[] options) { return null; }
        protected virtual IEnumerator OnClose() { return null; }
        protected virtual IEnumerator OnPromptVO(DialogPrompt prompt) { return null; }
        protected virtual IEnumerator OnResponseVO(DialogResponse response) { return null; }

    }

    [System.Serializable]
    public class DialogNode
    {
        [JsonIgnore]
        public Dialog Dialog { get; private set; }
        public string Id { get; set; }

        [JsonIgnore]
        public bool Used;

        [JsonIgnore]
        public Dictionary<string, float> From = new Dictionary<string, float>();

        [JsonInclude]
        private List<DialogPrompt> _prompts = new List<DialogPrompt>();

        [JsonInclude]
        private List<DialogResponse> _responses = new List<DialogResponse>();

        [JsonIgnore]
        public DialogPrompt CurrentPrompt { get; private set; }
        [JsonIgnore]
        public List<DialogResponse> CurrentResponses { get; private set; }


        [JsonIgnore]
        public int NumPrompts { get { return _prompts.Count; } }
        [JsonIgnore]
        public int NumResponses { get { return _responses.Count; } }
        
        public bool UsePickRandomValidPrompt { get; private set; }
        public int NumPickRandomResponses { get; private set; }

        public Rect EditorPosition = new Rect(50, 50, 400, 300);

        public DialogNode PickRandomValidPrompt()
        {
            UsePickRandomValidPrompt = true;
            return this;
        }

        public DialogNode PickRandomValidResponses(int count)
        {
            NumPickRandomResponses = count;
            return this;
        }

        public void RemovePrompt(DialogPrompt prompt)
        {
            _prompts.Remove(prompt);
        }

        public void RemoveResponse(DialogResponse response)
        {
            _responses.Remove(response);
        }

        public DialogPrompt[] GetPrompts()
        {
            return _prompts.ToArray();
        }

        public DialogResponse[] GetResponses()
        {
            return _responses.ToArray();
        }

        public bool ContainsResponse(DialogResponse response)
        {
            return _responses.Contains(response);
        }

        public DialogNode() { }

        public DialogNode(Dialog dialog, string id)
        {
            Dialog = dialog;
            Id = id;
            NumPickRandomResponses = -1;
            From = new Dictionary<string, float>();
        }


        public DialogPrompt AddPrompt(string text)
        {
            DialogPrompt prompt = new DialogPrompt(this, _prompts.Count, null, null, text);
            _prompts.Add(prompt);
            return prompt;
        }

        public DialogPrompt AddPrompt(string npc, string text)
        {
            DialogPrompt prompt = new DialogPrompt(this, _prompts.Count, npc, null, text);
            _prompts.Add(prompt);
            return prompt;
        }

        public DialogPrompt AddPrompt(string npc, string mood, string text)
        {
            DialogPrompt prompt = new DialogPrompt(this, _prompts.Count, npc, mood, text);
            _prompts.Add(prompt);
            return prompt;
        }

        public DialogResponse AddResponse(string text, string nextNodeId = null)
        {
            DialogResponse response = new DialogResponse(this, _responses.Count, text, nextNodeId);
            _responses.Add(response);
            return response;
        }

        internal IEnumerator MakeCurrent()
        {
            CurrentPrompt = GetEvaluatedPrompt();

            yield return CurrentPrompt.Execute();

            CurrentResponses = GetEvaluatedResponses();
        }

        private DialogPrompt GetEvaluatedPrompt()
        {
            List<DialogPrompt> validPrompts = new List<DialogPrompt>();
            for (int i = 0; i < _prompts.Count; i++)
            {
                DialogPrompt prompt = _prompts[i];
                
                if (prompt.Condition == null || prompt.Condition())
                {
                    if (!UsePickRandomValidPrompt)
                    {
                        return prompt;
                    }
                    else
                    {
                        validPrompts.Add(prompt);
                    }
                }
            }

            if (validPrompts.Count > 0)
                return validPrompts.Random();

            return null;
        }

        private List<DialogResponse> GetEvaluatedResponses()
        {
            List<DialogResponse> responses = new List<DialogResponse>();
            for (int i = 0; i < _responses.Count; i++)
            {
                DialogResponse response = _responses[i];

                if (response.Condition == null || response.Condition())
                {
                    response.Show();
                    responses.Add(response);
                }
            }

            if (NumPickRandomResponses > -1)
            {
                while (responses.Count > NumPickRandomResponses)
                {
                    responses.Remove(responses.Random());
                }
            }

            return responses;
        }
    }

    [System.Serializable]
    public class DialogPrompt
    {
        [NonSerialized]
        private DialogNode _node;
        
        public bool ShowFullEditor;

        public string Npc { get; set; }
        public string NpcMood { get; set; }
        public string Text { get; set; }
        
        public int Index { get; private set; }

        
        public Script ActionScript = new Script();
        public Script ConditionScript = new Script();

        [JsonIgnore]
        public Func<bool> Condition { get; private set; }

        [JsonIgnore]
        public Action<DialogPrompt> ShowAction { get; private set; }
        [JsonIgnore]
        public Func<DialogPrompt, IEnumerator> ShowActionBlocking { get; private set; }

        public DialogNode Done() { return _node; }

        public DialogPrompt SetCondition(Func<bool> condition)
        {
            Condition = condition;
            return this;
        }

        public DialogPrompt OnShow(Action<DialogPrompt> action)
        {
            ShowAction = action;
            return this;
        }

        public DialogPrompt OnShowBlocking(Func<DialogPrompt, IEnumerator> action)
        {
            ShowActionBlocking = action;
            return this;
        }

        public IEnumerator Execute()
        {
            if (ShowAction != null)
            {
                ShowAction(this);
            }
            else if (ShowActionBlocking != null)
            {
                yield return ShowActionBlocking(this);
            }

            yield return null;
        }

        public DialogPrompt() { }

        public DialogPrompt(DialogNode dialog, int index, string npc, string mood, string text)
        {
            Index = index;
            _node = dialog;
            Npc = npc;
            NpcMood = mood;
            Text = text;
        }
    }

    [Serializable]
    public class PropDictionary : SerialiazableDictionary<string, string> { }

    [System.Serializable]
    public class DialogResponse
    {
        [NonSerialized]
        private DialogNode _node;

        public bool ShowFullEditor;

        [JsonIgnore]
        public bool EditorDrawLine;

        public string Text { get; set; }
        
        public Script ActionScript = new Script();
        public Script ConditionScript = new Script();
        public Script DisableScript = new Script();
        public Script NextNodeScript = new Script();

        public enum NextNodeTypes { Id, End, Script };
        public NextNodeTypes NextNodeType;
        public string NextNodeId { get; set; }

        public int Index { get; private set; }

        [JsonIgnore]
        public Func<bool> Condition { get; private set; }

        [JsonIgnore]
        public Action<DialogResponse> SelectAction { get; private set; }
        [JsonIgnore]
        public Func<DialogResponse, IEnumerator> SelectActionBlocking { get; private set; }


        [JsonIgnore]
        public Action<DialogResponse> ShowAction { get; private set; }
        

        public DialogNode Done() { return _node; }

        public DialogResponse SetCondition(Func<bool> condition)
        {
            Condition = condition;
            return this;
        }

        public DialogResponse OnSelect(Action<DialogResponse> action)
        {
            SelectAction = action;
            return this;
        }

        public DialogResponse OnSelectBlocking(Func<DialogResponse, IEnumerator> action)
        {
            SelectActionBlocking = action;
            return this;
        }

        public DialogResponse OnShow(Action<DialogResponse> action)
        {
            ShowAction = action;
            return this;
        }

        public void Show()
        {
            if (ShowAction != null)
                ShowAction(this);
        }

        public IEnumerator Execute()
        {
            if (SelectAction != null)
            {
                SelectAction(this);
            }
            else if (SelectActionBlocking != null)
            {
                yield return SelectActionBlocking(this);
            }

            yield return null;
        }

        public DialogResponse() { }
        public DialogResponse(DialogNode dialog, int index, string text, string nextNodeId = null)
        {
            Index = index;
            _node = dialog;
            Text = text;
            NextNodeId = nextNodeId;
        }
    }

}
