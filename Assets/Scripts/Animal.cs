using UnityEngine;

public class Animal : Character, IFiniteStateMachine
{
    AnimalSO animalSO { get => characterSO as AnimalSO; }
    [SerializeField] State state;
    [SerializeField] StateMachine stateMachine;
    public State CurrentState
    {
        get => state;
        set => state = value;
    }
    public StateMachine Machine => stateMachine;
    private bool hasSons = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        hasSons = !animalSO.GetReproduction;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnValidate()
    {
        if (characterSO != null && !(characterSO is AnimalSO))
        {
            Debug.LogWarning("Warning! The Animal script only accepts a CharacterSO characterSO of the AnimalSO sub-class.");
            characterSO = null;
        }
    }
}
