using UnityEngine;
using System.Linq;

public interface IFiniteStateMachine
{
    State CurrentState { get; set; }
    StateMachine Machine { get; }

    public void SetState(State newState = null)
    {
        if (Machine == null) return;
        if (Machine.GetTransitions(CurrentState).Select(x => x.GetEndState).Contains(newState)) CurrentState = newState;
    }

    public void SetState(string newStateName)
    {
        if (Machine == null) return;
        if (string.IsNullOrEmpty(newStateName))
        {
            SetState((State)null);
            return;
        }
        SetState(GetStateAssets().FirstOrDefault(x => x.name == newStateName));
    }

    static State[] GetStateAssets()
    {
        return Resources.LoadAll<State>("");
    }
}
