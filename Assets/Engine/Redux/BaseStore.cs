using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
public abstract class BaseStore<TState, TAction> where TState : IState where TAction : IAction<TState>
{
    public TState State { get; private set; }

    private readonly Queue<TAction> _queuedActions;
    private readonly List<string> _history;
    protected BaseStore(TState state)
    {
        State = state;
        _history = new List<string>();
        _queuedActions = new Queue<TAction>();
        _lastQueueTime = Time.realtimeSinceStartup;

        LitJson.JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(Convert.ToDouble(obj)));
        LitJson.JsonMapper.RegisterImporter<double, float>(input => Convert.ToSingle(input));
    }

    public bool TrackHistory = true;

    private float _lastQueueTime;

    private bool _isProcessing;

    public IEnumerator Dispatch(string serializedAction)
    {
        var json = LitJson.JsonMapper.ToObject(serializedAction);
        var actionId = json["ActionType"].ToString();
        var type = Type.GetType(actionId);
        
        MethodInfo MI = typeof(LitJson.JsonMapper).GetMethods().Single((x) => x.IsGenericMethod && x.Name == "ToObject" && x.GetParameters().Single().ParameterType == typeof(string));
        var gen = MI.MakeGenericMethod(type);
        
        yield return Dispatch((TAction) gen.Invoke(null, new[] { serializedAction }));
    }

    private void InsertTimeDelta()
    {
        var newTime = Time.realtimeSinceStartup;
        var deltaTime = newTime - _lastQueueTime;
        _lastQueueTime = newTime;
        var waitAction = new InternalWaitAction(deltaTime);
        _history.Add(waitAction.ToString());
    }

    public IEnumerator Dispatch(TAction action)
    {
        if (_isProcessing)
        {
            if (TrackHistory)
            {
                InsertTimeDelta();
            }
            
            _queuedActions.Enqueue(action);
            yield break;
        }

        
        if (_queuedActions.Count == 0 && TrackHistory)
        {
            InsertTimeDelta();
        }
        
        _isProcessing = true;
        _queuedActions.Enqueue(action);
        while (_queuedActions.Count > 0)
        {
            action = _queuedActions.Dequeue();
            if (TrackHistory)
                _history.Add(action.ToString());
            yield return action.Run(State);
        }
        _isProcessing = false;
    }

    public List<string> GetHistory()
    {
        return _history.ToList();
    }
}