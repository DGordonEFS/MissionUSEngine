using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using DarkChariotStudios.Dialogs;

public class AdventureEngine : BaseStore<AdventureEngineState, AdventureEngineAction>
{
    private static AdventureEngine _instance;
    public static AdventureEngine GetInstance()
    {
        if (_instance == null)
            _instance = new AdventureEngine();
        return _instance;
    }

    public AdventureEngine() : base(new AdventureEngineState()) { }
}


public class AdventureEngineState : IState
{
    public VariableList GlobalVariables;
    public Dialog CurrentDialog;

    public AdventureEngineState()
    {
        GlobalVariables = Resources.Load<VariableListSO>("GlobalVariables").Variables;
    }
}