using UnityEngine;
using System.Collections.Generic;

public abstract class Item : MonoBehaviour
{
    public ItemSO itemData;
    public int currentStack = 1;
    public Vector2Int gridPosition;
    public int currentFoldIndex = 0;
    public int currentRotation = 0;
    public bool isFlipped = false;
    public Character owner;
    public float reloadQuantity = 1f;
    public abstract void Action(bool pressing);
    public abstract void Throw(bool pressing);
    public abstract bool CanUse();
    public abstract bool HandleReload(Item item);

    public List<Vector2Int> GetOccupiedCells(Vector2Int originPosition)
    {
        List<Vector2Int> calculatedCells = new List<Vector2Int>();
        ItemShape currentShape = itemData.GetFoldingConfigurations[currentFoldIndex];
        foreach (Vector2Int cell in currentShape.cells)
        {
            Vector2Int modifiedCell = cell;
            if (isFlipped) modifiedCell.x = -modifiedCell.x;
            switch (currentRotation % 4)
            {
                case 0:
                    break;
                case 1:
                    modifiedCell = new Vector2Int(modifiedCell.y, -modifiedCell.x);
                    break;
                case 2:
                    modifiedCell = new Vector2Int(-modifiedCell.x, -modifiedCell.y);
                    break;
                case 3:
                    modifiedCell = new Vector2Int(-modifiedCell.y, modifiedCell.x);
                    break;
            }
            calculatedCells.Add(originPosition + modifiedCell);
        }
        return calculatedCells;
    }

    public bool CanReload(out Item reloadItem)
    {
        if (itemData.GetReloadItem == null || owner == null || owner.inventoryGrid == null)
        {
            reloadItem = null;
            return false;
        }
        return owner.inventoryGrid.HasItem(itemData.GetReloadItem, out reloadItem);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CycleFoldOrFlipState()
    {
        if (isFlipped || itemData.GetFoldingConfigurations[currentFoldIndex].Symmetrical())
        {
            currentFoldIndex += 1;
            currentFoldIndex %= itemData.GetFoldingConfigurations.Length;
            isFlipped = false;
        }
        else isFlipped = true;
    }

    public void RotateItem(int n)
    {
        currentRotation = Character.ModFunc(currentRotation + n, 4);
    }

    public void Equip(Character.Hand hand)
    {
        transform.SetParent(hand.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        hand.item = this;
        owner = hand.character;
    }

    public void Reload()
    {
        Item reloadItem;
        if (!CanReload(out reloadItem)) return;
        if (!HandleReload(reloadItem)) return;
        reloadItem.currentStack -= 1;
        if (reloadItem.currentStack > 0) return;
        owner.inventoryGrid.RemoveItem(reloadItem);
        Destroy(reloadItem.gameObject);
    }

    public void Pick(Character character)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        for (int i = 0; i < character.hands.Length; i++)
        {
            if (character.hands[i].item == null)
            {
                character.hands[i].item = this;
                if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;
                return;
            }
        }
        if (character.inventoryGrid == null) return;
        Item itemInGrid;
        if (character.inventoryGrid.HasItem(itemData, out itemInGrid) && itemInGrid.currentStack < itemInGrid.itemData.GetMaxStack)
        {
            itemInGrid.currentStack += currentStack;
            if (itemInGrid.currentStack > itemInGrid.itemData.GetMaxStack)
            {
                currentStack = itemInGrid.currentStack - itemInGrid.itemData.GetMaxStack;
                itemInGrid.currentStack = itemInGrid.itemData.GetMaxStack;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        for (int x = 0; x < character.inventoryGrid.width; x++)
        {
            for (int y = 0; y < character.inventoryGrid.height; y++)
            {
                Vector2Int origin = new Vector2Int(x, y);
                if (character.inventoryGrid.PlaceItem(this, origin))
                {
                    if (rb != null) rb.constraints = RigidbodyConstraints.FreezeAll;
                    return;
                }
            }
        }
    }

    public void Drop()
    {
        gameObject.SetActive(true);
        transform.SetParent(null);
        bool wasInHands = false;
        if (owner != null)
        {
            for (int i = 0; i < owner.hands.Length; i++)
            {
                if (owner.hands[i].item == this)
                {
                    owner.hands[i].item = null;
                    wasInHands = true;
                    break;
                }
            }
        }
        if (!wasInHands && owner != null) owner.inventoryGrid.RemoveItem(this);
        owner = null;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.constraints = RigidbodyConstraints.None;
        for (int i = 1; i < currentStack; i++)
        {
            GameObject newItem = Instantiate(gameObject, transform.position, transform.rotation);
            Item newItemComponent = newItem.GetComponent<Item>();
            if (newItemComponent != null) newItemComponent.currentStack = 1;
        }
    }

    public Vector2Int GetItemCellSize()
    {
        ItemShape currentShape = itemData.GetFoldingConfigurations[currentFoldIndex];
        int width0 = 0, width1 = 0, height0 = 0, height1 = 0;
        foreach (Vector2Int cell in currentShape.cells)
        {
            if (cell.x < width0) width0 = cell.x;
            if (cell.x > width1) width1 = cell.x;
            if (cell.y < height0) height0 = cell.y;
            if (cell.y > height1) height1 = cell.y;
        }
        int rawWidth = width1 - width0 + 1;
        int rawHeight = height1 - height0 + 1;
        return new Vector2Int(rawWidth, rawHeight);
    }

    public Vector2Int GetRotatedItemCellSize()
    {
        Vector2Int baseSize = GetItemCellSize();
        if (currentRotation % 2 != 0) return new Vector2Int(baseSize.y, baseSize.x);
        return baseSize;
    }
}
