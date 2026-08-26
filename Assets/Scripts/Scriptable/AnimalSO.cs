using UnityEngine;

public interface IDistribution
{
    float Expectancy();
    float Variance();

    public float StandardDeviation()
    {
        return Mathf.Sqrt(Variance());
    }
}

[System.Serializable]
public struct FloatDistribution : IDistribution
{
    public enum DistType
    {
        Uniform,
        Exponential,
        Triangular,
        Erlang,
        Normal,
        LogNormal,
        Gamma,
        Weibull
    }

    public DistType distType;
    // Uniform, Triangular
    public float min, max;
    // Exponential
    public float lambda;
    // Triangular
    public float mode;
    // Erlang
    public int m;
    public float beta;
    // Normal, LogNormal
    public float mu, sigma;
    // Gamma, Weibull
    public float shape, scale;

    public float Evaluate()
    {
        switch (distType)
        {
            case DistType.Uniform: return RandomDistribution.Uniform(min, max);
            case DistType.Exponential: return RandomDistribution.Exponential(lambda);
            case DistType.Triangular: return RandomDistribution.Triangular(min, mode, max);
            case DistType.Erlang: return RandomDistribution.Erlang(m, beta);
            case DistType.Normal: return RandomDistribution.Normal(mu, sigma);
            case DistType.LogNormal: return RandomDistribution.LogNormal(mu, sigma);
            case DistType.Gamma: return RandomDistribution.Gamma(shape, scale);
            case DistType.Weibull: return RandomDistribution.Weibull(shape, scale);
            default: return 0f;
        }
    }

    public float Expectancy()
    {
        switch (distType)
        {
            case DistType.Uniform: return (min + max) / 2f;
            case DistType.Exponential: return 1f / lambda;
            case DistType.Triangular: return (min + mode + max) / 3f;
            case DistType.Erlang: return m * beta;
            case DistType.Normal: return mu;
            case DistType.LogNormal: return mu;
            case DistType.Gamma: return shape * scale;
            case DistType.Weibull: return scale * Mathf.Gamma(1f + 1f / shape);
            default: return 0f;
        }
    }

    private float Variance()
    {
        switch (distType)
        {
            case DistType.Uniform: return Mathf.Pow(max - min, 2f) / 12f;
            case DistType.Exponential: return 1f / (lambda * lambda);
            case DistType.Triangular: return (min * min + max * max + mode * mode - min * max - min * mode - max * mode) / 18f;
            case DistType.Erlang: return m * beta * beta;
            case DistType.Normal: return sigma * sigma;
            case DistType.LogNormal: return sigma * sigma;
            case DistType.Gamma: return shape * scale * scale;
            case DistType.Weibull: return scale * scale * (Mathf.Gamma(1f + 2f / shape) - Mathf.Pow(Mathf.Gamma(1f + 1f / shape), 2f));
            default: return 0f;
        }
    }
}

[System.Serializable]
public struct IntDistribution : IDistribution
{
    public enum DistType
    {
        Poisson,
        Binomial,
        NegBinomial
    }

    public DistType distType;
    // Poisson
    public float lambda;
    // Binomial
    public int n;
    // NegBinomial
    public int r;
    // Binomial, NegBinomial
    [Range(0f, 1f)]
    public float p;

    public int Evaluate()
    {
        switch (distType)
        {
            case DistType.Poisson: return RandomDistribution.Poisson(lambda);
            case DistType.Binomial: return RandomDistribution.Binomial(n, p);
            case DistType.NegBinomial: return RandomDistribution.NegBinomial(r, p);
            default: return 0;
        }
    }

    public float Expectancy()
    {
        switch (distType)
        {
            case DistType.Poisson: return lambda;
            case DistType.Binomial: return n * p;
            case DistType.NegBinomial: return r * (1f - p) / p;
            default: return 0f;
        }
    }

    private float Variance()
    {
        switch (distType)
        {
            case DistType.Poisson: return lambda;
            case DistType.Binomial: return n * p * (1f - p);
            case DistType.NegBinomial: return r * (1f - p) / (p * p);
            default: return 0f;
        }
    }
}

[System.Serializable]
public struct RangeDistribution
{
    public FloatDistribution min;
    public FloatDistribution max;
}

[CreateAssetMenu(fileName = "AnimalSO", menuName = "Scriptable Objects/CharacterSO/AnimalSO")]
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
            if (cumulativeBiomass >= randomValue) return species[i];
        }
        return null;
    }
}
