using UnityEngine;

[CreateAssetMenu(fileName = "AnimalSO", menuName = "Scriptable Objects/CharacterSO/AnimalSO/Base")]
public class AnimalSO : CharacterSO
{
    [SerializeField]
    CharacterSO[] preys;
    [SerializeField]
    Vector3 meanScale;
    [SerializeField]
    Vector3 stdDevScale;
    [SerializeField]
    FloatDistribution mass;
    [Header("Reproduction Stats")]
    [SerializeField]
    FloatDistribution lifeExpectancy;
    [SerializeField]
    IntDistribution sons;
    [SerializeField, Range(0f, 1f)]
    float reproductionChance = 0.5f;
    [SerializeField]
    RangeDistribution childbearingAge;
    public CharacterSO[] GetPreys { get => preys; }
    public Vector3 GetScale
    {
        get => new Vector3(
            RandomDistribution.Normal(meanScale.x, stdDevScale.x),
            RandomDistribution.Normal(meanScale.y, stdDevScale.y),
            RandomDistribution.Normal(meanScale.z, stdDevScale.z)
        );
    }
    public Vector3 GetExpectedScale { get => meanScale; }
    public Vector3 GetStdDevScale { get => stdDevScale; }
    public float GetMass { get => mass.Evaluate(); }
    public float GetExpectedMass { get => mass.Expectancy(); }
    public float GetStandardDeviationMass { get => mass.StandardDeviation(); }
    public bool GetReproduction { get => RandomDistribution.Bernoulli(reproductionChance); }
    public int GetSons { get => sons.Evaluate(); }
    public float GetExpectedSons { get => sons.Expectancy(); }
    public float GetStandardDeviationSons { get => sons.StandardDeviation(); }
    public float GetLifeExpectancy { get => lifeExpectancy.Evaluate(); }
    public float GetExpectedLifeExpectancy { get => lifeExpectancy.Expectancy(); }
    public float GetStandardDeviationLifeExpectancy { get => lifeExpectancy.StandardDeviation(); }
    public Vector2 GetChildbearingAge
    {
        get => new Vector2(
            childbearingAge.min.Evaluate(),
            childbearingAge.max.Evaluate()
        );
    }
    public Vector2 GetExpectedChildbearingAge
    {
        get => new Vector2(
            childbearingAge.min.Expectancy(),
            childbearingAge.max.Expectancy()
        );
    }
    public Vector2 GetStandardDeviationChildbearingAge
    {
        get => new Vector2(
            childbearingAge.min.StandardDeviation(),
            childbearingAge.max.StandardDeviation()
        );
    }

    public static AnimalSO GetRandomSpecies(AnimalSO[] species)
    {
        if (species == null || species.Length == 0) return null;
        float totalBiomass = 0f;
        float[] biomasses = new float[species.Length];
        for (int i = 0; i < species.Length; i++)
        {
            AnimalSO sp = species[i];
            float biomass = 0;
            for (int j = 0; j < sp.GetSons; j++) biomass += sp.GetLifeExpectancy * sp.GetMass;
            biomasses[i] = biomass;
            totalBiomass += biomass;
        }
        float randomValue = RandomDistribution.Uniform(0f, totalBiomass);
        float cumulativeBiomass = 0f;
        for (int i = 0; i < species.Length; i++)
        {
            cumulativeBiomass += biomasses[i];
            if (cumulativeBiomass >= randomValue) return (species[i] is ReproductiveCasteSO) ? ((ReproductiveCasteSO)species[i]).GetSon : species[i];
        }
        return null;
    }
}
