using System;
using UnityEngine;
using System.Collections;

public enum ItemAction
{
    Action,
    Throw
}

public class Character : MonoBehaviour
{
    [Header("Character")]
    public Rigidbody rb;
    public Camera cam;
    public bool isRunning = false;
    public float height;
    public CharacterSO characterSO;
    [Header("Combat")]
    public float health;
    [Header("Inventory")]
    public Item currentItem { get => hands[currentHandIndex].item; }
    public Hand[] hands;
    [SerializeField] protected Item[] startingItems;
    [HideInInspector] public InventoryGrid inventoryGrid;
    private Coroutine actionCoroutine;
    private Coroutine throwCoroutine;
    protected int currentHandIndex = 0;

    public static int ModFunc(int a, int b)
    {
        return a < 0 ? ModFunc(a + b, b) : a % b;
    }

    [System.Serializable]
    public struct Hand
    {
        public Character character;
        public Transform transform;
        public Item item;
    }

    public bool CanSee(Vector3 target)
    {
        if (characterSO == null) return false;
        Room chRoom = RoomST.Instance.ClosestRoom(transform.position),
        tgRoom = RoomST.Instance.ClosestRoom(target);
        PortalGate gate;
        bool sameRoom = chRoom == tgRoom,
        connectedRooms = chRoom.Connected(tgRoom, out gate);
        if (!sameRoom && !connectedRooms) return false;
        Vector3 roomDirection = sameRoom ? target - transform.position : Vector3.zero,
        portalDirection = connectedRooms ? gate.portal.Direction(transform.position, target) : Vector3.zero;
        float roomDistance = sameRoom ? roomDirection.magnitude : float.MaxValue,
        portalDistance = connectedRooms ? portalDirection.magnitude : float.MaxValue,
        visionAngleCos = Mathf.Cos(characterSO.GetVisionAngle * Mathf.Deg2Rad);
        bool canRoom = roomDistance <= characterSO.GetVisionLength && Vector3.Dot(cam.transform.forward, roomDirection.normalized) >= visionAngleCos,
        canPortal = portalDistance <= characterSO.GetVisionLength && Vector3.Dot(cam.transform.forward, portalDirection.normalized) >= visionAngleCos;
        RaycastHit hit;
        if (canRoom) return Portal.Raycast(new Ray(transform.position, roomDirection.normalized), out hit, characterSO.GetVisionLength);
        if (canPortal) return Portal.Raycast(new Ray(transform.position, portalDirection.normalized), out hit, characterSO.GetVisionLength);
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        health = characterSO.GetMaxHealth;
        for (int i = 0; i < hands.Length; i++)
        {
            hands[i].character = this;
            if (hands[i].item != null) hands[i].item.owner = this;
        }
        if (inventoryGrid == null) inventoryGrid = gameObject.AddComponent<InventoryGrid>();
        inventoryGrid.owner = this;
        foreach (Item item in startingItems) if (item != null) PickCopy(item);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Move(Vector2 velocity)
    {
        if (rb == null) return;
        Vector2 input = velocity.normalized * (isRunning ? characterSO.GetRunSpeed : characterSO.GetMoveSpeed);
        rb.linearVelocity = input.x * rb.transform.right + rb.linearVelocity.y * Vector3.up + input.y * rb.transform.forward;
    }

    public void Jump()
    {
        if (rb == null) return;
        Ray ray = new Ray(transform.position + Vector3.down * height, Vector3.down);
        if (!Physics.Raycast(ray, 0.1f)) return;
        float initialSpeed = Mathf.Sqrt(2 * characterSO.GetJumpHeight * -Physics.gravity.y);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, initialSpeed, rb.linearVelocity.z);
    }

    public void Look(Vector2 delta)
    {
        if (cam == null) return;
        rb.transform.Rotate(Vector3.up, delta.x);
        float currentPitch = cam.transform.localRotation.eulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;
        float newPitch = currentPitch - delta.y;
        newPitch = Mathf.Clamp(newPitch, characterSO.GetLimit.x, characterSO.GetLimit.y);
        cam.transform.localRotation = Quaternion.Euler(newPitch, 0f, 0f);
    }

    public void SwitchHand(int n)
    {
        currentHandIndex = ModFunc(currentHandIndex + n, hands.Length);
    }

    public void SetHandItem(int index, Item item)
    {
        if (index < 0 || index >= hands.Length) return;
        Item item0 = hands[index].item;
        hands[index].item = item;
        if (item0 != null && item0 != item) item0.gameObject.SetActive(false);
        if (item != null)
        {
            item.Equip(hands[index]);
            item.gameObject.SetActive(true);
        }
    }

    public void HandleAction(bool isPressing, ItemAction action = ItemAction.Action)
    {
        if (currentItem == null) return;
        Coroutine coroutine = action == ItemAction.Action ? actionCoroutine : throwCoroutine;
        if (isPressing)
        {
            if (coroutine != null) StopCoroutine(coroutine);
            Coroutine routine = StartCoroutine(ActionRoutine(action));
            if (action == ItemAction.Action) actionCoroutine = routine;
            else throwCoroutine = routine;
        }
        else
        {
            if (coroutine != null) StopCoroutine(coroutine);
            if (action == ItemAction.Action) currentItem.Action(false);
            else currentItem.Throw(false);
        }
    }

    public void PickItem(Item item)
    {
        item.Pick(this);
    }

    private void PickCopy(Item item)
    {
        Item itemCopy = Instantiate(item.gameObject).GetComponent<Item>();
        itemCopy.currentStack = item.currentStack;
        PickItem(itemCopy);
    }

    public void DropItem(Item item)
    {
        if (item.owner == this) item.Drop();
    }

    public void DropCurrentItem()
    {
        if (currentItem != null) DropItem(currentItem);
    }

    public void Reload()
    {
        if (currentItem != null) currentItem.Reload();
    }

    private IEnumerator ActionRoutine(ItemAction action = ItemAction.Action)
    {
        while (true)
        {
            if (currentItem == null) yield break;
            if (!currentItem.CanUse()) yield break;
            if (action == ItemAction.Action) currentItem.Action(true);
            else currentItem.Throw(true);
            yield return null;
        }
    }
}
