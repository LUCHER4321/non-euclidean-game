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
            Texture2D iconTexture = shape.icon;
            iconDisplay.texture = iconTexture;
            iconDisplay.enabled = true;
            float ratio = item.AspectRatio();
            if (ratio > 1f) iconDisplay.rectTransform.localScale = new Vector3(1f, 1f / ratio, 1f);
            else iconDisplay.rectTransform.localScale = new Vector3(ratio, 1f, 1f);
        }
        else
        {
            iconDisplay.texture = null;
            iconDisplay.enabled = false;
        }
    }
}
