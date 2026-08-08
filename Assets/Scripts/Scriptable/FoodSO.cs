using UnityEngine;

[CreateAssetMenu(fileName = "FoodSO", menuName = "Scriptable Objects/ItemSO/FoodSO")]
public class FoodSO : ItemSO
{
    [SerializeField] float healthRestore = 10f;

    public float GetHealthRestore { get => healthRestore; }
}
