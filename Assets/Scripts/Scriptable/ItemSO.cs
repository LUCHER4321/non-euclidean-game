using UnityEngine;

[System.Serializable]
public struct ItemShape
{
    public Vector2Int[] cells;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    [SerializeField]
    LanText itemName;
    [SerializeField]
    int maxStack = 1;
    [SerializeField]
    ItemShape[] foldingConfigurations;
    [SerializeField]
    GameObject prefab;
    public string GetItemName { get => itemName.GetText(); }
    public int GetMaxStack { get => maxStack; }
    public ItemShape[] GetFoldingConfigurations { get => foldingConfigurations; }
    public GameObject GetPrefab { get => prefab; }
}
