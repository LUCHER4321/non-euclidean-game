using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ItemUI : MonoBehaviour
{
    [SerializeField] TMP_Text stackText;
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
        if (item.currentStack > 1)
        {
            stackText.gameObject.SetActive(true);
            stackText.text = item.currentStack.ToString();
            ItemShape currentShape = item.itemData.GetFoldingConfigurations[item.currentFoldIndex];
            Vector2Int lastCell = currentShape.LastCell();
            int shapeMinX = int.MaxValue, shapeMaxX = int.MinValue;
            int shapeMinY = int.MaxValue, shapeMaxY = int.MinValue;
            foreach (Vector2Int cell in currentShape.cells)
            {
                if (cell.x < shapeMinX) shapeMinX = cell.x;
                if (cell.x > shapeMaxX) shapeMaxX = cell.x;
                if (cell.y < shapeMinY) shapeMinY = cell.y;
                if (cell.y > shapeMaxY) shapeMaxY = cell.y;
            }
            int shapeWidth = shapeMaxX - shapeMinX + 1;
            int shapeHeight = shapeMaxY - shapeMinY + 1;
            float cellMinX = (float)(lastCell.x - shapeMinX) / shapeWidth;
            float cellMaxX = (float)(lastCell.x - shapeMinX + 1) / shapeWidth;
            float cellMinY = (float)(lastCell.y - shapeMinY) / shapeHeight;
            float cellMaxY = (float)(lastCell.y - shapeMinY + 1) / shapeHeight;
            float textMinX = cellMinX + (cellMaxX - cellMinX) / 2f;
            float textMaxX = cellMaxX;
            float textMinY = cellMinY;
            float textMaxY = cellMinY + (cellMaxY - cellMinY) / 2f;
            stackText.rectTransform.anchorMin = new Vector2(textMinX, textMinY);
            stackText.rectTransform.anchorMax = new Vector2(textMaxX, textMaxY);
            stackText.rectTransform.offsetMin = Vector2.zero;
            stackText.rectTransform.offsetMax = Vector2.zero;
            stackText.rectTransform.localRotation = Quaternion.Euler(0, 0, 90f * item.currentRotation);
            Vector3 textScale = Vector3.one;
            if (item.isFlipped) textScale.x = -1;
            stackText.rectTransform.localScale = textScale;
        }
        else stackText.gameObject.SetActive(false);
    }
}
