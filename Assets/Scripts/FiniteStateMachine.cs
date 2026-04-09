using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    public void SetState(string newStateName)
    {
        if (stateMachine == null) return;
        if (newStateName == "")
        {
            SetState(newState: null);
            return;
        }
#if UNITY_EDITOR
        {
            SetState(GetStateAssetsEditorOnly().FirstOrDefault(x => x.name == newStateName));
            return;
        }
#endif
        SetState(GetStateAssets().FirstOrDefault(x => x.name == newStateName));
    }

#if UNITY_EDITOR
    static State[] GetStateAssetsEditorOnly()
    {
        string[] guids = AssetDatabase.FindAssets("t:State");
        State[] states = new State[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            states[i] = AssetDatabase.LoadAssetAtPath<State>(path);
        }
        return states;
    }
#endif

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
