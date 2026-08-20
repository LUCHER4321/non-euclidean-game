using UnityEngine;

[System.Serializable]
public struct FloatDistribution
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
}

[System.Serializable]
public struct IntDistribution
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
    public float GetMass { get => mass.Evaluate(); }
    public bool GetReproduction { get => RandomDistribution.Bernoulli(reproductionChance); }
    public float GetLifeExpectancy { get => lifeExpectancy.Evaluate(); }
    public float[] GetChildbearingAge
    {
        get => new float[] {
            childbearingAge.min.Evaluate(),
            childbearingAge.max.Evaluate()
        };
    }
}
