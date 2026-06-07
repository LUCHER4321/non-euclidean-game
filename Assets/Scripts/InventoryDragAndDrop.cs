using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryDragAndDrop : InventoryUI
{
    [Header("Input Actions")]
    [SerializeField]
    InputActionReference clickAction;
    [SerializeField]
    InputActionReference rightClickAction;
    [SerializeField]
    InputActionReference scrollAction;
    [SerializeField]
    InputActionReference pointerPositionAction;
    private Item currentlyDraggingItem;

    void Awake()
    {
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlyDraggingItem != null)
        {
            Vector2 pointerPos = pointerPositionAction.action.ReadValue<Vector2>();
            currentlyDraggingItem.transform.position = pointerPos;
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!Player.Instance.inventory) return;
        Vector2 pointerPos = pointerPositionAction.action.ReadValue<Vector2>();
        Vector2Int gridPos = GetGridPositionFromMouse(pointerPos);
        if (currentlyDraggingItem == null)
        {
            Item itemToPick = GetItemAt(gridPos);
            if (itemToPick != null)
            {
                currentlyDraggingItem = itemToPick;
                RemoveItem(itemToPick);
            }
        }
        else if (CanPlaceItem(currentlyDraggingItem, gridPos))
        {
            PlaceItem(currentlyDraggingItem, gridPos);
            currentlyDraggingItem = null;
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (!Player.Instance.inventory) return;
        if (currentlyDraggingItem != null) currentlyDraggingItem.CycleFoldOrFlipState();
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (!Player.Instance.inventory) return;
        if (currentlyDraggingItem != null)
        {
            float scrollValue = context.ReadValue<Vector2>().y;
            if (scrollValue > 0) currentlyDraggingItem.RotateItem(1);
            else if (scrollValue < 0) currentlyDraggingItem.RotateItem(-1);
        }
    }
}
