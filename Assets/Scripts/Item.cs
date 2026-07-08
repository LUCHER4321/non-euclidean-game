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
    public abstract void Action(bool pressing);
    public abstract void Throw(bool pressing);
    public abstract bool CanUse();
    public abstract void Reload();

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

    public float AspectRatio()
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
        return (float)(width1 - width0 + 1) / (height1 - height0 + 1);
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
        if (isFlipped)
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
    }
}
