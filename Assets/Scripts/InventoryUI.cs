using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class InventoryUI : InventoryGrid
{
    [Header("Colors Settings")]
    [SerializeField]
    Color[] baseColors = new Color[]
    {
        new Color(0.2f, 0.2f, 0.2f, 0.6f),
        new Color(0.3f, 0.3f, 0.3f, 0.6f)
    };
    [SerializeField]
    Color[] usedColors = new Color[]
    {
        new Color(0.2f, 0.2f, 0f, 1f),
        new Color(0.3f, 0.3f, 0f, 1f)
    };
    [Header("UI Prefabs")]
    [SerializeField] ItemUI itemUIPrefab;
    [SerializeField] Transform cells;
    public static Vector2 baseCellSize = new Vector2(64f, 64f);

    private Dictionary<Item, ItemUI> itemUIDictionary = new Dictionary<Item, ItemUI>();
    private Image[,] images;

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
        UpdateColors();
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
        images = new Image[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cellObj = new GameObject($"Cell_{x}_{y}", typeof(RectTransform), typeof(Image));
                cellObj.transform.SetParent(cells != null ? cells : transform, false);
                RectTransform cellRect = cellObj.GetComponent<RectTransform>();
                images[x, y] = cellObj.GetComponent<Image>();
                float minX = (float)x / (float)width;
                float maxX = (float)(x + 1) / (float)width;
                float minY = (float)(height - 1 - y) / (float)height;
                float maxY = (float)(height - y) / (float)height;
                cellRect.anchorMin = new Vector2(minX, minY);
                cellRect.anchorMax = new Vector2(maxX, maxY);
                cellRect.offsetMin = Vector2.zero;
                cellRect.offsetMax = Vector2.zero;
                baseCellSize = new Vector2(cellRect.rect.width, cellRect.rect.height);
                Color[] colors = GetItemAt(new Vector2Int(x, y)) == null ? baseColors : usedColors;
                images[x, y].color = colors[(x + y) % colors.Length];
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
        UpdateColors();
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

    public void UpdateColors()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color[] colors = GetItemAt(new Vector2Int(x, y)) == null ? baseColors : usedColors;
                images[x, y].color = colors[(x + y) % colors.Length];
            }
        }
    }

    public void TemporalColors(Vector2Int[] positions, Color[] colors)
    {
        UpdateColors();
        foreach (Vector2Int pos in positions) if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height) images[pos.x, pos.y].color = colors[(pos.x + pos.y) % colors.Length];
    }
}
