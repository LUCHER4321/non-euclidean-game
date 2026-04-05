using UnityEngine;

[CreateAssetMenu(fileName = "Transition", menuName = "Scriptable Objects/State Machines/Transition")]
public class Transition : ScriptableObject
{
    [SerializeField]
    State startState;
    [SerializeField]
    State endState;
    public State GetStartState { get => startState; }
    public State GetEndState { get => endState; }
}
