using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IntDistribution))]
public class IntDistributionDrawer : DistributionDrawer
{
    protected override int GetAdditionalLineCount(int enumValueIndex)
    {
        switch (enumValueIndex)
        {
            case 0: // Poisson: lambda
                return 1;
            case 1: // Binomial: n, p
                return 2;
            case 2: // NegBinomial: r, p
                return 2;
            default:
                return 0;
        }
    }

    protected override void DrawDistributionFields(ref Rect rect, SerializedProperty property, int enumValueIndex)
    {
        switch (enumValueIndex)
        {
            case 0: // Poisson
                DrawField(ref rect, property, "lambda");
                break;
            case 1: // Binomial
                DrawField(ref rect, property, "n");
                DrawField(ref rect, property, "p");
                break;
            case 2: // NegBinomial
                DrawField(ref rect, property, "r");
                DrawField(ref rect, property, "p");
                break;
        }
    }
}