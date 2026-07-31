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
    public CharacterSO characterSO;
    [Header("Combat")]
    public float health;
    public Item currentItem { get => hands[currentHandIndex].item; }
    public Hand[] hands;
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

    public bool CanSee(GameObject target)
    {
        if (characterSO == null) return false;
        Vector3 direction = target.transform.position - cam.transform.position;
        if (direction.sqrMagnitude > Mathf.Pow(characterSO.GetVisionLength, 2)) return false;
        if (Vector3.Dot(cam.transform.forward, direction.normalized) <= Mathf.Cos(characterSO.GetVisionAngle * Mathf.Deg2Rad)) return false;
        if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit, characterSO.GetVisionLength))
        {
            return hit.collider.gameObject == target;
        }
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
        Ray ray = new Ray(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, characterSO.GetHeight + 0.1f)) return;
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
        if (isPressing)
        {
            if (actionCoroutine != null) StopCoroutine(action == ItemAction.Action ? actionCoroutine : throwCoroutine);
            Coroutine routine = StartCoroutine(ActionRoutine(action));
            if (action == ItemAction.Action) actionCoroutine = routine;
            else throwCoroutine = routine;
        }
        else
        {
            if (actionCoroutine != null) StopCoroutine(action == ItemAction.Action ? actionCoroutine : throwCoroutine);
            if (action == ItemAction.Action) currentItem.Action(false);
            else currentItem.Throw(false);
        }
    }

    public void PickItem(Item item)
    {
        item.Pick(this);
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
