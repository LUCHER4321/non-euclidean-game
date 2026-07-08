using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class InventoryDragAndDrop : InventoryUI
{
    [Header("Input Actions")]
    [SerializeField] InputActionReference pointerPositionAction;
    [Header("UI Hands")]
    [SerializeField] HandSlotUI[] handSlots;
    [Header("UI Dragging")]
    [SerializeField] RawImage dragIconDisplay;
    private Item currentlyDraggingItem;
    private Coroutine dragCoroutine;

    void Awake()
    {
        base.Awake();
        if (dragIconDisplay != null) dragIconDisplay.enabled = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        UpdateAllHandSlots();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        UpdateAllHandSlots();
        if (currentlyDraggingItem != null) StartDragging(currentlyDraggingItem);
    }
    
    void OnDisable()
    {
        if (dragCoroutine != null) StopCoroutine(dragCoroutine);
        if (dragIconDisplay != null) dragIconDisplay.enabled = false;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!Player.Instance.inventory) return;
        Vector2 pointerPos = pointerPositionAction.action.ReadValue<Vector2>();
        foreach (HandSlotUI slot in handSlots)
        {
            if (slot.IsMouseOver(pointerPos))
            {
                HandleHandSlotClick(slot.handIndex);
                return;
            }
        }
        Vector2Int gridPos = GetGridPositionFromMouse(pointerPos);
        if (currentlyDraggingItem == null)
        {
            Item itemToPick = GetItemAt(gridPos);
            if (itemToPick != null)
            {
                RemoveItem(itemToPick);
                StartDragging(itemToPick);
            }
        }
        else if (CanPlaceItem(currentlyDraggingItem, gridPos))
        {
            PlaceItem(currentlyDraggingItem, gridPos);
            StopDragging();
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (!Player.Instance.inventory) return;
        if (currentlyDraggingItem != null) 
        {
            currentlyDraggingItem.CycleFoldOrFlipState();
            UpdateDragVisuals();
        }
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (!Player.Instance.inventory) return;
        if (currentlyDraggingItem != null)
        {
            float scrollValue = context.ReadValue<Vector2>().y;
            if (scrollValue > 0) currentlyDraggingItem.RotateItem(1);
            else if (scrollValue < 0) currentlyDraggingItem.RotateItem(-1);
            UpdateDragVisuals();
        }
    }

    void HandleHandSlotClick(int handIndex)
    {
        Item currentItemInHand = Player.Instance.hands[handIndex].item;
        if (currentlyDraggingItem != null)
        {
            if (currentItemInHand == null)
            {
                Player.Instance.SetHandItem(handIndex, currentlyDraggingItem);
                StopDragging();
            }
            else
            {
                Item tempItem = currentItemInHand;
                Player.Instance.SetHandItem(handIndex, currentlyDraggingItem);
                StartDragging(tempItem);
            }
        }
        else if (currentItemInHand != null)
        {
            Player.Instance.SetHandItem(handIndex, null);
            StartDragging(currentItemInHand);
        }
        UpdateAllHandSlots();
    }

    public void UpdateAllHandSlots()
    {
        if (Player.Instance == null || Player.Instance.hands == null) return;
        foreach (HandSlotUI slot in handSlots)
        {
            Item itemInHand = Player.Instance.hands[slot.handIndex].item;
            slot.UpdateSlotDisplay(itemInHand);
        }
    }

    void StartDragging(Item item)
    {
        currentlyDraggingItem = item;
        if (dragIconDisplay != null)
        {
            dragIconDisplay.enabled = true;
            UpdateDragVisuals();
        }

        if (dragCoroutine != null) StopCoroutine(dragCoroutine);
        dragCoroutine = StartCoroutine(FollowMouseRoutine());
    }

    void StopDragging()
    {
        currentlyDraggingItem = null;
        if (dragIconDisplay != null)
        {
            dragIconDisplay.texture = null;
            dragIconDisplay.enabled = false;
        }

        if (dragCoroutine != null)
        {
            StopCoroutine(dragCoroutine);
            dragCoroutine = null;
        }
    }

    IEnumerator FollowMouseRoutine()
    {
        while (currentlyDraggingItem != null)
        {
            if (pointerPositionAction != null && dragIconDisplay != null)
            {
                Vector2 pointerPos = pointerPositionAction.action.ReadValue<Vector2>();
                dragIconDisplay.rectTransform.position = pointerPos;
            }
            yield return null;
        }
        if (dragIconDisplay != null) dragIconDisplay.enabled = false;
    }

    void UpdateDragVisuals()
    {
        if (dragIconDisplay == null || currentlyDraggingItem == null || currentlyDraggingItem.itemData == null) return;
        ItemShape shape = currentlyDraggingItem.itemData.GetFoldingConfigurations[currentlyDraggingItem.currentFoldIndex];
        dragIconDisplay.texture = shape.icon;
        dragIconDisplay.rectTransform.localRotation = Quaternion.Euler(0, 0, -90f * currentlyDraggingItem.currentRotation);
        Vector2Int cellSize = currentlyDraggingItem.GetItemCellSize();
        float targetWidth = cellSize.x * baseCellSize.x;
        float targetHeight = cellSize.y * baseCellSize.y;
        dragIconDisplay.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        float scaleX = currentlyDraggingItem.isFlipped ? -1f : 1f;
        dragIconDisplay.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
    }
}
