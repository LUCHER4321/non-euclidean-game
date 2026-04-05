using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateMachine", menuName = "Scriptable Objects/State Machines/StateMachine")]
public class StateMachine : ScriptableObject
{
    [SerializeField]
    Transition[] transitions;
    public Transition[] GetTransitions(State state)
    {
        List<Transition> trs = new List<Transition>();
        foreach (Transition t in transitions)
        {
            if (t.GetStartState == state) trs.Add(t);
        }
        return trs.ToArray();
    }
}
