using UnityEngine;
using System.Collections.Generic;

public static class FluxEventDispatcher
{
    private static List<AdventureEngineAction> _history = new List<AdventureEngineAction>();

    public static AdventureEngineAction[] GetHistory() { return _history.ToArray(); }

    public delegate void FluxEvent(AdventureEngineAction action);
    public static event FluxEvent OnFluxEvent;

    public static void Trigger(AdventureEngineAction action)
    {
        _history.Add(action);
        if (OnFluxEvent != null)
            OnFluxEvent(action);
    }
}
