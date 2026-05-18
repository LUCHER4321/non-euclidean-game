using UnityEngine;
using System.Linq;

public class FiniteStateMachine : MonoBehaviour
{
    [SerializeField]
    State state;
    [SerializeField]
    StateMachine stateMachine;
    public State GetState { get => state; }

    public void SetState(State newState = null)
    {
        if (stateMachine == null) return;
        if (stateMachine.GetTransitions(state).Select(x => x.GetEndState).Contains(newState)) state = newState;
    }

    public void SetState(string newStateName)
    {
        if (stateMachine == null) return;
        if (newStateName == "")
        {
            SetState();
            return;
        }
        SetState(GetStateAssets().FirstOrDefault(x => x.name == newStateName));
    }

    static State[] GetStateAssets()
    {
        return Resources.LoadAll<State>("");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
