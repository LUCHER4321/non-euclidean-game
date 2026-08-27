using UnityEngine;

[System.Serializable]
public struct CasteSon
{
    public AnimalSO species;
    public float weight;
}

[CreateAssetMenu(fileName = "ReproductiveCasteSO", menuName = "Scriptable Objects/CharacterSO/AnimalSO/ReproductiveCasteSO")]
public class ReproductiveCasteSO : AnimalSO
{
    [SerializeField] CasteSon[] sonsSpecies;
    public AnimalSO GetSon { get => RandomSon(); }

    private AnimalSO RandomSon()
    {
        float totalWeight = 0f;
        foreach (CasteSon son in sonsSpecies) totalWeight += son.weight;
        float random = RandomDistribution.Uniform(0f, totalWeight);
        float cumulativeWeight = 0f;
        foreach (CasteSon son in sonsSpecies)
        {
            cumulativeWeight += son.weight;
            if (random <= cumulativeWeight) return son.species;
        }
        return null;
    }
}
