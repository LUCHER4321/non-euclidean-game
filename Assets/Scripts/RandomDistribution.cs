using UnityEngine;
using System.Linq;

public interface IDistribution
{
    float Expectancy();
    float Variance();
}

public static class DistributionExtensions
{
    public static float StandardDeviation<T>(this T distribution) where T : IDistribution
    {
        return Mathf.Sqrt(distribution.Variance());
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
            case DistType.Weibull: return scale * Factorial(1f / shape);
            default: return 0f;
        }
    }

    public float Variance()
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
            case DistType.Weibull: return scale * scale * (Factorial(2f / shape) - Mathf.Pow(Factorial(1f / shape), 2f));
            default: return 0f;
        }
    }

    private static readonly double[] LanczosCoefficients = {
        1.000000000190015,
        76.18009172947146,
        -86.50532032941677,
        24.01409824083091,
        -1.231739572450155,
        0.1208650973866179e-2,
        -0.5395239384953e-5
    };

    private static readonly double LogSqrtTwoPi = (double)Mathf.Log(Mathf.Sqrt(2f * Mathf.PI));

    private static double LogGamma(double z)
    {
        if (z < 0.5) return (double)Mathf.Log(Mathf.PI / Mathf.Sin(Mathf.PI * (float)z)) - LogGamma(1.0 - z);
        z -= 1.0;
        double x = LanczosCoefficients[0];
        for (int i = 1; i < LanczosCoefficients.Length; i++) x += LanczosCoefficients[i] / (z + i);
        double t = z + 5.5;
        return LogSqrtTwoPi + (double)Mathf.Log((float)x) + (z + 0.5) * (double)Mathf.Log((float)t) - t;
    }

    private static double Gamma(double z)
    {
        return (double)Mathf.Exp((float)LogGamma(z));
    }

    private static float Factorial(float x)
    {
        if (x > 0f && x < 1f) return (float)Gamma((double)x + 1.0);
        if (x == 0f) return 1f;
        if (x >= 1f) return x * Factorial(x - 1f);
        if (x < 0f) return x % 1f == 0f ? 0f : Factorial(x + 1f) / (x + 1f);
        return 0f;
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

    public float Variance()
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

public class RandomDistribution : MonoBehaviour
{
    public static float Uniform(float min = 0f, float max = 1f)
    {
        return min + (max - min) * Random.value;
    }

    public static float Exponential(float lambda = 1f)
    {
        return -Mathf.Log(1f - Random.value) / lambda;
    }

    public static float Triangular(float min = 0f, float mode = 0.5f, float max = 1f)
    {
        float x = Uniform(min, max);
        float fx = 2f * (x >= min && x <= mode ? (x - min) / (mode - min) : x >= mode && x <= max ? (max - x) / (max - mode) : 0f) / (max - min);
        float u = Random.value;
        float acceptCondition = fx * (max - min) / 2f;
        if (u > acceptCondition) return Triangular(min, mode, max);
        return x;
    }

    public static float Erlang(int m = 1, float beta = 1f)
    {
        float prod = 1f;
        for (int i = 0; i < m; i++) prod *= Random.value;
        return -Mathf.Log(prod) * beta / m;
    }

    public static float Normal(float mu = 0f, float sigma = 1f)
    {
        float[] u = new float[] { Random.value, Random.value };
        float[] v = u.Select(x => 2f * x - 1f).ToArray();
        float w = v.Select(x => x * x).Sum();
        if (w > 1f) return Normal(mu, sigma);
        float y = Mathf.Sqrt(-2f * Mathf.Log(w) / w);
        return mu + sigma * v[0] * y;
    }

    public static float LogNormal(float mu = 0f, float sigma = 1f)
    {
        float mu2 = mu * mu, sigma2 = sigma * sigma;
        float normalValues = Normal(Mathf.Log(mu2 / Mathf.Sqrt(sigma2 + mu2)), Mathf.Sqrt(Mathf.Log(1 + sigma2 / mu2)));
        return Mathf.Exp(normalValues);
    }

    public static int Poisson(float lambda)
    {
        float a = Mathf.Exp(-lambda);
        float b = 1f;
        int i = 0;
        while (true)
        {
            b *= Random.value;
            if (b < a) return i;
            i++;
        }
    }

    public static bool Bernoulli(float p = 0.5f)
    {
        return Random.value < p;
    }

    public static int Binomial(int n = 1, float p = 0.5f)
    {
        int s = 0;
        for (int i = 0; i < n; i++) if (Bernoulli(p)) s++;
        return s;
    }

    public static int NegBinomial(int r = 1, float p = 0.5f)
    {
        int successes = 0;
        int failures = 0;
        while (successes < r)
        {
            if (Bernoulli(p)) successes++;
            else failures++;
        }
        return failures;
    }

    private static float e = Mathf.Exp(1f);

    public static float Gamma(float shape = 1f, float scale = 1f)
    {
        if (shape == 1f) return -scale * Mathf.Log(Random.value);
        if (shape < 1f)
        {
            float[] ua = new float[] { Random.value, Random.value };
            float b = e + shape / e;
            float p = b * ua[0];
            float x = p <= 1f ? Mathf.Pow(p, 1 / shape) : -Mathf.Log((b - p) / shape);
            if (ua[1] > (p <= 1f ? Mathf.Exp(-x) : Mathf.Pow(x, shape - 1f))) return Gamma(shape, scale);
            return x * scale;
        }
        float d = shape - 1f / 3f;
        float c = 1f / Mathf.Sqrt(9f * d);
        float z = Normal();
        float v = 1f + c * z;
        v *= v * v;
        if (v <= 0) return Gamma(shape, scale);
        float u = Random.value;
        float z2 = z * z;
        if (u < 1f - 0.0331f * z2 * z2 || Mathf.Log(u) < 0.5f * z2 + d * (1f - v + Mathf.Log(v))) return d * v * scale;
        return Gamma(shape, scale);
    }

    public static float Weibull(float shape = 1f, float scale = 1f)
    {
        return scale * Mathf.Pow(-Mathf.Log(1f - Random.value), 1f / shape);
    }
}
