using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemUI : MonoBehaviour
{
    private RectTransform rectTransform;
    private RawImage rawImage;
    public Item linkedItem { get; private set; }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();
        rawImage.raycastTarget = false; 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(Item item, int gridWidth, int gridHeight)
    {
        linkedItem = item;
        rawImage.texture = item.itemData.GetFoldingConfigurations[item.currentFoldIndex].icon;
        List<Vector2Int> cells = item.GetOccupiedCells(item.gridPosition);
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (Vector2Int cell in cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }
        float anchorMinX = (float)minX / gridWidth;
        float anchorMaxX = (float)(maxX + 1) / gridWidth;
        float anchorMinY = (float)(gridHeight - 1 - maxY) / gridHeight;
        float anchorMaxY = (float)(gridHeight - minY) / gridHeight;
        float centerX = (anchorMinX + anchorMaxX) / 2f;
        float centerY = (anchorMinY + anchorMaxY) / 2f;
        Vector2Int rawSize = item.GetItemCellSize();
        float spanX = (float)rawSize.x / gridWidth;
        float spanY = (float)rawSize.y / gridHeight;
        rectTransform.anchorMin = new Vector2(centerX - (spanX / 2f), centerY - (spanY / 2f));
        rectTransform.anchorMax = new Vector2(centerX + (spanX / 2f), centerY + (spanY / 2f));
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localRotation = Quaternion.Euler(0, 0, -90f * item.currentRotation);
        Vector3 scale = Vector3.one;
        if (item.isFlipped) scale.x = -1;
        rectTransform.localScale = scale;
    }
}
