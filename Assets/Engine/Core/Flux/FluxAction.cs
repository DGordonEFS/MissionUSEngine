using UnityEngine;
using System.Collections;

public abstract class AdventureEngineAction : IAction<AdventureEngineState>
{
    public string ActionType { get; private set; }

    public AdventureEngineAction()
    {
        ActionType = GetType().Name;
    }

    public IEnumerator Run(AdventureEngineState state)
    {
        yield return OnRun(state);
    }

    protected virtual IEnumerator OnRun(AdventureEngineState state)
    {
        yield return null;
    }

    public override string ToString()
    {
        return LitJson.JsonMapper.ToJson(this);
    }
}


public class ReduxLoadGameAction : AdventureEngineAction
{
    public string Variables;

    public ReduxLoadGameAction(string variables)
    {
        Variables = variables;
    }
}

public class ReduxLoadStateAction : AdventureEngineAction
{
    public string StateId { get; private set; }

    public ReduxLoadStateAction(string stateId)
    {
        StateId = stateId;
    }
}

public class SetVariableAction : AdventureEngineAction
{
    public string Id { get; private set; }
    public string Value { get; private set; }
    public string Type { get; private set; }

    public SetVariableAction(string id, string value, VariableData.VariableTypes type=VariableData.VariableTypes.String)
    {
        Id = id;
        Value = value;
        Type = System.Enum.GetName(typeof(VariableData.VariableTypes), type);
    }

    public SetVariableAction(string id, bool value)
    {
        Id = id;
        Value = value.ToString();
        Type = System.Enum.GetName(typeof(VariableData.VariableTypes), VariableData.VariableTypes.Bool);
    }

    public SetVariableAction(string id, float value)
    {
        Id = id;
        Value = value.ToString();
        Type = System.Enum.GetName(typeof(VariableData.VariableTypes), VariableData.VariableTypes.Number);
    }

    public SetVariableAction() { }

    protected override IEnumerator OnRun(AdventureEngineState state)
    {
        state.GlobalVariables.SetVariable(Id, Value, (VariableData.VariableTypes)System.Enum.Parse(typeof(VariableData.VariableTypes), Type));
        yield break;
    }
}

public class DialogResponseAction : AdventureEngineAction
{
    public int ResponseIndex;

    public DialogResponseAction() { }

    public DialogResponseAction(int responseIndex)
    {
        ResponseIndex = responseIndex;
    }

    protected override IEnumerator OnRun(AdventureEngineState state)
    {
        yield return AdventureEngine.GetInstance().State.CurrentDialog.SelectResponse(ResponseIndex);
    }
}

public class OpenDialogAction : AdventureEngineAction
{
    public string DialogId { get; private set; }
    public string StartingId { get; private set; }

    public OpenDialogAction(string dialogId, string startingId)
    {
        DialogId = dialogId;
        StartingId = startingId;
    }

    public OpenDialogAction() { }

    public OpenDialogAction(string dialogId)
    {
        DialogId = dialogId;
    }

    protected override IEnumerator OnRun(AdventureEngineState state)
    {
        yield return null;
        // set the data
        /*AdventureEngine.GetInstance().State.CurrentDialog = Resources.Load<DarkChariotStudios.Dialogs.Dialog>(DialogId);

        var defaultNodeId = StartingId;
        if (defaultNodeId == null || defaultNodeId == "")
            defaultNodeId = AdventureEngine.GetInstance().State.CurrentDialog.DefaultNodeId;
        
        yield return AdventureEngine.GetInstance().State.CurrentDialog.SetCurrentNode(defaultNodeId);

        var dialogPlayer = GameObject.FindObjectOfType<DialogPlayer>();
        
        yield return dialogPlayer.Open(DialogId, StartingId);
        
        // wait while the player is opening
        yield return new WaitUntil(() => dialogPlayer.IsOpen);
        */
    }
}

public class CloseDialogAction : AdventureEngineAction
{
    public CloseDialogAction() { }

    protected override IEnumerator OnRun(AdventureEngineState state)
    {
        var dialogPlayer = GameObject.FindObjectOfType<DialogPlayer>();

        yield return dialogPlayer.Close();
        // set the data
        AdventureEngine.GetInstance().State.CurrentDialog = null;
    }
}

public class WaitAction : AdventureEngineAction
{
    public float Time { get; private set; }

    public WaitAction() { }

    public WaitAction(float time)
    {
        Time = time;
    }

    protected override IEnumerator OnRun(AdventureEngineState state)
    {
        yield return new WaitForSeconds(Time);
    }
}

public class InternalWaitAction : WaitAction
{
    public InternalWaitAction() { }
    public InternalWaitAction(float time) : base(time) { }
}