using UnityEngine;
using System.Linq;

public interface IFiniteStateMachine
{
    State CurrentState { get; set; }
    StateMachine Machine { get; }
}

public static class StateMachineExtensions
{
    public static void SetState<T>(this T fsm, State newState) where T : IFiniteStateMachine
    {
        if (fsm.Machine == null) return;
        if (fsm.Machine.GetTransitions(fsm.CurrentState).Select(x => x.GetEndState).Contains(newState)) fsm.CurrentState = newState;
    }

    public static void SetState<T>(this T fsm, string newStateName) where T : IFiniteStateMachine
    {
        if (fsm.Machine == null) return;
        if (string.IsNullOrEmpty(newStateName))
        {
            fsm.SetState((State)null);
            return;
        }
        fsm.SetState(GetStateAssets().FirstOrDefault(x => x.name == newStateName));
    }

    public static State[] GetStateAssets()
    {
        return Resources.LoadAll<State>("");
    }
}
