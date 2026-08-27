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
    private bool hasSons;
    private Vector3 scale;
    private float mass, lifeExpectancy, age;
    private Vector2 childbearingAge;

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
        height = scale.y;
        mass = animalSO.GetMass;
        if (rb != null) rb.mass = mass;
        lifeExpectancy = animalSO.GetLifeExpectancy;
        childbearingAge = animalSO.GetChildbearingAge;
        age = RandomDistribution.Triangular(0f, RandomDistribution.Uniform(childbearingAge.x, childbearingAge.y), lifeExpectancy);
    }

    // Update is called once per frame
    void Update()
    {
        age += Time.deltaTime / secondsPerYear;
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
