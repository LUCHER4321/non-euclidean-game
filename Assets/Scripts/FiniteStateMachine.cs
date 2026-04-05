using UnityEngine;
using System.Linq;

public class FiniteStateMachine : MonoBehaviour
{
    [SerializeField]
    State state;
    [SerializeField]
    StateMachine stateMachine;
    public State GetState { get => state; }

    public void SetState(State newState)
    {
        if (stateMachine == null) return;
        if (stateMachine.GetTransitions(state).Select(x => x.GetEndState).Contains(newState)) state = newState;
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
