using UnityEngine;

[System.Serializable]
public struct ItemShape
{
    public Vector2Int[] cells;
    public Texture2D icon;

    public bool Symmetrical()
    {
        if (cells.Length <= 1) return true;
        foreach (Vector2Int cell in cells) if (cell.x != cells[0].x && cell.y != cells[0].y) return false;
        return true;
    }
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    [SerializeField] LanText itemName;
    [SerializeField] int maxStack = 1;
    [SerializeField] ItemShape[] foldingConfigurations;
    [SerializeField] GameObject prefab;
    [SerializeField] ItemSO reloadItem;
    public string GetItemName { get => itemName.GetText(); }
    public int GetMaxStack { get => maxStack; }
    public ItemShape[] GetFoldingConfigurations { get => foldingConfigurations; }
    public GameObject GetPrefab { get => prefab; }
    public ItemSO GetReloadItem { get => reloadItem; }
}
