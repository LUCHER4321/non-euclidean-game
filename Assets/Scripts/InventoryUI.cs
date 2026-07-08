using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class InventoryUI : InventoryGrid
{
    [Header("Colors Settings")]
    [SerializeField] Color color0 = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    [SerializeField] Color color1 = new Color(0.3f, 0.3f, 0.3f, 0.6f);
    [Header("UI Prefabs")]
    [SerializeField] ItemUI itemUIPrefab;

    private Dictionary<Item, ItemUI> itemUIDictionary = new Dictionary<Item, ItemUI>();

    public Vector2Int GetGridPositionFromMouse(Vector2 screenPosition)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, null, out Vector2 localPoint);
        float normalizedX = (localPoint.x - rectTransform.rect.xMin) / rectTransform.rect.width;
        float normalizedY = (localPoint.y - rectTransform.rect.yMin) / rectTransform.rect.height;
        int x = Mathf.FloorToInt(normalizedX * width);
        int y = Mathf.FloorToInt((1.0f - normalizedY) * height);
        return new Vector2Int(x, y);
    }

    public new bool PlaceItem(Item item, Vector2Int origin)
    {
        bool success = base.PlaceItem(item, origin);
        if (success) CreateOrUpdateItemUI(item);
        return success;
    }

    protected virtual void Awake()
    {
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        AdjustInventoryAspectRatio();
        GenerateGridCells();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void AdjustInventoryAspectRatio()
    {
        AspectRatioFitter fitter = GetComponent<AspectRatioFitter>();
        if (fitter == null) fitter = gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = (float)width / (float)height;
    }

    private void GenerateGridCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cellObj = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                cellObj.transform.SetParent(transform, false);
                RectTransform cellRect = cellObj.GetComponent<RectTransform>();
                Image cellImage = cellObj.GetComponent<Image>();
                float minX = (float)x / (float)width;
                float maxX = (float)(x + 1) / (float)width;
                float minY = (float)(height - 1 - y) / (float)height;
                float maxY = (float)(height - y) / (float)height;
                cellRect.anchorMin = new Vector2(minX, minY);
                cellRect.anchorMax = new Vector2(maxX, maxY);
                cellRect.offsetMin = Vector2.zero;
                cellRect.offsetMax = Vector2.zero;
                if ((x + y) % 2 == 0) cellImage.color = color0;
                else cellImage.color = color1;
            }
        }
    }

    public new void RemoveItem(Item item)
    {
        base.RemoveItem(item);
        if (itemUIDictionary.TryGetValue(item, out ItemUI ui))
        {
            Destroy(ui.gameObject);
            itemUIDictionary.Remove(item);
        }
    }

    public void CreateOrUpdateItemUI(Item item)
    {
        if (!itemUIDictionary.TryGetValue(item, out ItemUI ui))
        {
            ui = Instantiate(itemUIPrefab, transform);
            itemUIDictionary.Add(item, ui);
        }
        ui.Setup(item, width, height);
    }
}
