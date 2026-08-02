using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Player : Character
{
    public static Player Instance { get; private set; }
    [Header("Player")]
    public PlayerInput playerInput;
    public bool inventory = false;
    [SerializeField] bool pause = false;
    [SerializeField] GameObject pauseMenu;
    public RebindableAction currentRebind;
    [SerializeField] Material damageEffect;
    [Header("Items")]
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] SphereCollider itemDetector;
    [SerializeField] HandSigner handSigner;
    [SerializeField] TMP_Text pickableItemText;
    [SerializeField] LanText pickableItemLanText;
    [Header("FPS")]
    [SerializeField] TMP_Text fpsText;
    [SerializeField] LanText fpsLanText;
    [SerializeField] float fps;
    [Range(0f, 1f)]
    [SerializeField] float fpsSmoothing = 0.1f;
    private int healthPropertyID;
    private float lastHealth = -1f;
    private InputAction moveAction;
    private Item pickableItem;

    public bool cursorVisible { get => pause || inventory; }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        healthPropertyID = Shader.PropertyToID("_Health");
        fps = 1f / Time.unscaledDeltaTime;
        inventoryGrid = inventoryUI;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        if (RoomST.Instance != null)
        {
            transform.position = RoomST.Instance.GetRandomSpawnPoint();
            transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
        cam.transform.localRotation = Quaternion.identity;
        moveAction = playerInput.actions["Move"];
        if (handSigner != null) handSigner.UpdateHands(currentHandIndex);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMove(moveAction.ReadValue<Vector2>());
        if (health != lastHealth) UpdateHealth();
        UpdateFPS();
        SetPickableItem();
    }

    void UpdateFPS()
    {
        int lastFPS = Mathf.CeilToInt(fps);
        float currentFPS = 1f / Time.unscaledDeltaTime;
        fps = Mathf.Lerp(fps, currentFPS, fpsSmoothing);
        if (fpsText != null && lastFPS != Mathf.CeilToInt(fps)) fpsText.text = fpsLanText.GetText(Mathf.CeilToInt(fps).ToString());
    }

    void UpdateHealth()
    {
        float healthPercent = health / characterSO.GetMaxHealth;
        damageEffect.SetFloat(healthPropertyID, healthPercent);
        lastHealth = health;
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (cursorVisible) return;
        isRunning = !context.canceled;
    }

    public void PlayerMove(Vector2 input)
    {
        if (cursorVisible) return;
        Move(input.normalized);
    }

    public void PlayerJump()
    {
        if (cursorVisible) return;
        Jump();
    }

    public void PlayerLook(InputAction.CallbackContext context)
    {
        if (cursorVisible) return;
        Vector2 input0 = context.ReadValue<Vector2>();
        bool[] inverts = OptionsMenuST.Instance.GetInverts;
        float invertX = inverts[0] ? -1f : 1f;
        float invertY = inverts[1] ? -1f : 1f;
        Vector2 input = OptionsMenuST.Instance.GetSensitivity * new Vector2(input0.x * invertX, input0.y * invertY);
        Look(input);
    }

    public void PlayerSwitchHand(InputAction.CallbackContext context)
    {
        if (cursorVisible || !context.performed) return;
        int n = context.ReadValue<Vector2>().y > 0f ? 1 : -1;
        SwitchHand(n);
        if (handSigner != null) handSigner.UpdateHands(currentHandIndex);
    }

    public void ToggleInput()
    {
        inventory = !inventory;
        inventoryUI.gameObject.SetActive(inventory);
        UpdateCursor();
        if (inventory)
        {
            InventoryDragAndDrop dragAndDrop = inventoryUI as InventoryDragAndDrop;
            if (dragAndDrop != null) dragAndDrop.UpdateAllHandSlots();
        }
    }

    public void ControlsChanged()
    {
        Debug.Log("Controls changed to " + playerInput.currentControlScheme);
    }

    public void OnItemAction(InputAction.CallbackContext context)
    {
        if (cursorVisible) return;
        if (currentItem == null) return;
        if (!context.performed) HandleAction(context.started, ItemAction.Action);
    }

    public void OnItemThrow(InputAction.CallbackContext context)
    {
        if (cursorVisible) return;
        if (currentItem == null) return;
        if (!context.performed) HandleAction(context.started, ItemAction.Throw);
    }

    public void PickThatItem()
    {
        SetPickableItem();
        if (pickableItem == null) return;
        PickItem(pickableItem);
    }

    void SetPickableItem()
    {
        RaycastHit hit;
        float maxScale = Mathf.Max(Mathf.Max(Mathf.Abs(itemDetector.transform.lossyScale.x), Mathf.Abs(itemDetector.transform.lossyScale.y)), Mathf.Abs(itemDetector.transform.lossyScale.z));
        if (!Portal.Raycast(new Ray(cam.transform.position, cam.transform.forward), out hit, itemDetector.radius * maxScale)) return;
        pickableItem = Portal.FirstParent(hit.collider.transform).GetComponent<Item>();
        if (pickableItemText == null || pickableItemLanText == null) return;
        pickableItemText.gameObject.SetActive(pickableItem != null);
        if (pickableItem != null) pickableItemText.text = pickableItemLanText.GetText(pickableItem.itemData.GetItemName);
    }

    public void Pause()
    {
        if (inventory)
        {
            ToggleInput();
            return;
        }
        pause = !pause;
        pauseMenu.SetActive(pause);
        if (pause) OptionsMenuST.Instance.Controls(false);
        UpdateCursor();
    }

    void UpdateCursor()
    {
        Cursor.visible = cursorVisible;
        Cursor.lockState = cursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
