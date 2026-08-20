using UnityEngine;
using System.Linq;

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
