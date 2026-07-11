using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    public static Player Instance { get; private set; }
    [Header("Player")]
    public PlayerInput playerInput;
    public bool inventory = false;
    public RebindableAction currentRebind;
    [SerializeField] Material damageEffect;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] float fps;
    private int healthPropertyID;
    private float lastHealth = -1f;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
        healthPropertyID = Shader.PropertyToID("_Health");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        Cursor.visible = inventory;
        Cursor.lockState = inventory ? CursorLockMode.None : CursorLockMode.Locked;
        if (RoomST.Instance != null)
        {
            transform.position = RoomST.Instance.GetRandomSpawnPoint();
            transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }
        cam.transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMove(playerInput.actions["Move"].ReadValue<Vector2>());
        if (health != lastHealth) UpdateHealth();
        fps = 1f / Time.unscaledDeltaTime;
    }

    void UpdateHealth()
    {
        float healthPercent = health / characterSO.GetMaxHealth;
        damageEffect.SetFloat(healthPropertyID, healthPercent);
        lastHealth = health;
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (inventory) return;
        isRunning = !context.canceled;
    }

    public void PlayerMove(Vector2 input)
    {
        if (inventory) return;
        Move(input.normalized);
    }

    public void PlayerJump()
    {
        if (inventory) return;
        Jump();
    }

    public void PlayerLook(InputAction.CallbackContext context)
    {
        if (inventory) return;
        Vector2 input0 = context.ReadValue<Vector2>();
        bool[] inverts = OptionsMenuST.Instance.GetInverts;
        float[] invertsNum = new float[2];
        for (int i = 0; i < 2; i++) invertsNum[i] = inverts[i] ? -1 : 1;
        Vector2 input = OptionsMenuST.Instance.GetSensitivity * new Vector2(input0.x * invertsNum[0], input0.y * invertsNum[1]);
        Look(input);
    }

    public void ToggleInput()
    {
        inventory = !inventory;
    inventoryUI.gameObject.SetActive(inventory);
    Cursor.visible = inventory;
    Cursor.lockState = inventory ? CursorLockMode.None : CursorLockMode.Locked;
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
        if (inventory) return;
        if (currentItem == null) return;
        if (!context.performed) HandleAction(context.started, ItemAction.Action);
    }

    public void OnItemThrow(InputAction.CallbackContext context)
    {
        if (inventory) return;
        if (currentItem == null) return;
        if (!context.performed) HandleAction(context.started, ItemAction.Throw);
    }
}
