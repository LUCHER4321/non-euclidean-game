using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatDistribution))]
public class FloatDistributionDrawer : DistributionDrawer
{
    protected override int GetAdditionalLineCount(int enumValueIndex)
    {
        switch (enumValueIndex)
        {
            case 0: // Uniform: min, max
                return 2;
            case 1: // Exponential: lambda
                return 1;
            case 2: // Triangular: min, max, mode
                return 3;
            case 3: // Erlang: m, beta
                return 2;
            case 4: // Normal: mu, sigma
                return 2;
            case 5: // LogNormal: mu, sigma
                return 2;
            case 6: // Gamma: shape, scale
                return 2;
            case 7: // Weibull: shape, scale
                return 2;
            default:
                return 0;
        }
    }

    protected override void DrawDistributionFields(ref Rect rect, SerializedProperty property, int enumValueIndex)
    {
        switch (enumValueIndex)
        {
            case 0: // Uniform
                DrawField(ref rect, property, "min");
                DrawField(ref rect, property, "max");
                break;
            case 1: // Exponential
                DrawField(ref rect, property, "lambda");
                break;
            case 2: // Triangular
                DrawField(ref rect, property, "min");
                DrawField(ref rect, property, "max");
                DrawField(ref rect, property, "mode");
                break;
            case 3: // Erlang
                DrawField(ref rect, property, "m");
                DrawField(ref rect, property, "beta");
                break;
            case 4: // Normal
            case 5: // LogNormal
                DrawField(ref rect, property, "mu");
                DrawField(ref rect, property, "sigma");
                break;
            case 6: // Gamma
            case 7: // Weibull
                DrawField(ref rect, property, "shape");
                DrawField(ref rect, property, "scale");
                break;
        }
    }
}