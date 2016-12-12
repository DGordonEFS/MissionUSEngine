using System.Collections;

public interface IAction<TState> where TState : IState
{
    string ActionType { get; }
    IEnumerator Run(TState state);
}