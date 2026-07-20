using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("Measures")]
    public int width = 10;
    public int height = 8;
    [HideInInspector]
    public Character owner;
    private Item[,] grid;

    public bool CanPlaceItem(Item item, Vector2Int origin)
    {
        List<Vector2Int> targetCells = item.GetOccupiedCells(origin);
        foreach (Vector2Int cell in targetCells)
        {
            if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height) return false;
            if (grid[cell.x, cell.y] != null)
            {
                Item existingItem = grid[cell.x, cell.y];
                if (existingItem.itemData == item.itemData && existingItem.currentStack + item.currentStack <= existingItem.itemData.GetMaxStack) continue;
                return false;
            }
        }
        return true;
    }

    public bool PlaceItem(Item item, Vector2Int origin)
    {
        if (!CanPlaceItem(item, origin)) return false;
        item.owner = owner;
        List<Vector2Int> targetCells = item.GetOccupiedCells(origin);
        item.gridPosition = origin;
        foreach (Vector2Int cell in targetCells)
        {
            Item existingItem = grid[cell.x, cell.y];
            if (existingItem != null && existingItem.itemData == item.itemData)
            {
                existingItem.currentStack += item.currentStack;
                Destroy(item.gameObject);
                return true;
            }
            grid[cell.x, cell.y] = item;
        }
        if (item.gameObject != null) item.gameObject.SetActive(false);
        return true;
    }

    public Item GetItemAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) return null;
        return grid[pos.x, pos.y];
    }

    protected virtual void Awake()
    {
        grid = new Item[width, height];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RemoveItem(Item item)
    {
        List<Vector2Int> occupiedCells = item.GetOccupiedCells(item.gridPosition);
        foreach (Vector2Int cell in occupiedCells) if (grid[cell.x, cell.y] == item) grid[cell.x, cell.y] = null;
        item.owner = null;
    }
}
