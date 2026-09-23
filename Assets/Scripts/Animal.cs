using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public struct StateEvent
{
    public State state;
    public UnityEvent onEnter, onDuring, onExit;
}

public class Animal : Character, IFiniteStateMachine
{
    AnimalSO animalSO { get => characterSO as AnimalSO; }
    [SerializeField] StateMachine stateMachine;
    [SerializeField] StateEvent[] events;
    public State CurrentState
    {
        get => state;
        set => state = value;
    }
    public StateMachine Machine => stateMachine;
    private State state = null;
    private bool hasSons;
    private Vector3 scale;
    private float mass, lifeExpectancy, age;
    private Vector2 childbearingAge;
    private Animal couple;
    private Animal[] hunters, preys, oponents;

    private Vector3 NormalizedScale { get => VectorDiv(scale, animalSO.GetExpectedScale); }
    private static float secondsPerYear = 365.25f * 24f * 3600f;

    private static Vector3 VectorDiv(Vector3 v0, Vector3 v1)
    {
        return new Vector3(v0.x / v1.x, v0.y / v1.y, v0.z / v1.z);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        hasSons = !animalSO.GetReproduction;
        scale = animalSO.GetScale;
        height = scale.y / 2f;
        mass = animalSO.GetMass;
        if (rb != null) rb.mass = mass;
        lifeExpectancy = animalSO.GetLifeExpectancy;
        childbearingAge = animalSO.GetChildbearingAge;
        age = RandomDistribution.Triangular(0f, RandomDistribution.Uniform(childbearingAge.x, childbearingAge.y), lifeExpectancy);
        TransitionState("Idle");
    }

    // Update is called once per frame
    void Update()
    {
        age += Time.deltaTime / secondsPerYear;
        StateEvent? stateEvent = events.FirstOrDefault(x => x.state == CurrentState);
        if (stateEvent != null) stateEvent.Value.onDuring?.Invoke();
    }

    void OnValidate()
    {
        if (characterSO != null && !(characterSO is AnimalSO))
        {
            Debug.LogWarning("Warning! The Animal script only accepts a CharacterSO characterSO of the AnimalSO sub-class.");
            characterSO = null;
        }
    }

    void Reproduce(Animal other)
    {
        if (CurrentState.GetStateName != "Reproduce" || other.CurrentState.GetStateName != "Reproduce" || other.animalSO != animalSO || other.hasSons || age < childbearingAge.x || other.age < other.childbearingAge.x || other.age > other.childbearingAge.y) return;
        if (hasSons || age > childbearingAge.y)
        {
            TransitionState("Idle");
            return;
        }
        AnimalSO[] sons = new AnimalSO[animalSO.GetSons];
        for (int i = 0; i < sons.Length; i++) sons[i] = (animalSO is ReproductiveCasteSO) ? ((ReproductiveCasteSO)animalSO).GetSon : animalSO;
    }

    void TransitionState(State newState)
    {
        State oldState = CurrentState;
        this.SetState(newState);
        if (oldState == CurrentState) return;
        StateEvent? oldStateEvent = events.FirstOrDefault(x => x.state == oldState),
        newStateEvent = events.FirstOrDefault(x => x.state == CurrentState);
        if (oldStateEvent != null) oldStateEvent.Value.onExit?.Invoke();
        if (newStateEvent != null) newStateEvent.Value.onEnter?.Invoke();
    }

    void TransitionState(string newStateName)
    {
        if (string.IsNullOrEmpty(newStateName))
        {
            TransitionState((State)null);
            return;
        }
        State newState = StateMachineExtensions.GetStateAssets().FirstOrDefault(x => x.name == newStateName);
        TransitionState(newState);
    }

    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private Vector3 currentTargetPosition = Vector3.negativeInfinity;

    void MoveToPosition(Vector3 position, float tolerance = 0.5f)
    {
        if (currentTargetPosition != position || currentPath == null || currentPath.Count == 0)
        {
            currentTargetPosition = position;
            currentPath = PathFinderST.GetPath(transform.position, position);
            currentPathIndex = 0;
        }
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            Move(Vector2.zero);
            return;
        }
        Vector3 targetWaypoint = currentPath[currentPathIndex];
        Vector3 directionToWaypoint = targetWaypoint - transform.position;
        if (directionToWaypoint.magnitude < tolerance)
        {
            currentPathIndex++;
            if (currentPathIndex >= currentPath.Count)
            {
                Move(Vector2.zero);
                return;
            }
            targetWaypoint = currentPath[currentPathIndex];
            directionToWaypoint = targetWaypoint - transform.position;
        }
        Vector3 localDirection = transform.InverseTransformDirection(directionToWaypoint.normalized);
        Vector2 moveInput = new Vector2(localDirection.x, localDirection.z).normalized;
        Move(moveInput);
    }
}
