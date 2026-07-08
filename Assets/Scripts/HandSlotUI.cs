using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class HandSlotUI : MonoBehaviour
{
    public int handIndex = 0;
    [Header("UI References")]
    [SerializeField] RawImage iconDisplay;

    private RectTransform rectTransform;

    public bool IsMouseOver(Vector2 screenPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition);
    }
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (iconDisplay == null) iconDisplay = GetComponentInChildren<RawImage>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSlotDisplay(Item item)
    {
        if (iconDisplay == null) return;
        
        if (item != null && item.itemData != null)
        {
            ItemShape shape = item.itemData.GetFoldingConfigurations[item.currentFoldIndex];
            iconDisplay.texture = shape.icon;
            iconDisplay.enabled = true;
            Vector2Int cellSize = item.GetItemCellSize();
            float targetWidth = cellSize.x * InventoryUI.baseCellSize.x;
            float targetHeight = cellSize.y * InventoryUI.baseCellSize.y;
            iconDisplay.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
            iconDisplay.rectTransform.localScale = Vector3.one;
            iconDisplay.rectTransform.localRotation = Quaternion.Euler(0, 0, -90f * item.currentRotation);
        }
        else
        {
            iconDisplay.texture = null;
            iconDisplay.enabled = false;
        }
    }
}
