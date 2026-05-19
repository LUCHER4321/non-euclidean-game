using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    public static Player Instance { get; private set; }
    [Header("Player")]
    public PlayerInput playerInput;
    public bool inputEnabled = true;
    public RebindableAction currentRebind;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = !inputEnabled;
        Cursor.lockState = inputEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        cam.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMove(playerInput.actions["Move"].ReadValue<Vector2>());
    }

    public void Run(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;
        isRunning = !context.canceled;
    }

    public void PlayerMove(Vector2 input)
    {
        if (!inputEnabled) return;
        Move(input.normalized);
    }

    public void PlayerJump()
    {
        if (!inputEnabled) return;
        Jump();
    }

    public void PlayerLook(InputAction.CallbackContext context)
    {
        if (!inputEnabled) return;
        Vector2 input0 = context.ReadValue<Vector2>();
        bool[] inverts = OptionsMenuST.Instance.GetInverts;
        float[] invertsNum = new float[2];
        for (int i = 0; i < 2; i++) invertsNum[i] = inverts[i] ? -1 : 1;
        Vector2 input = OptionsMenuST.Instance.GetSensitivity * new Vector2(input0.x * invertsNum[0], input0.y * invertsNum[1]);
        Look(input);
    }

    public void ToggleInput()
    {
        inputEnabled = !inputEnabled;
        Cursor.visible = !inputEnabled;
        Cursor.lockState = inputEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        playerInput.SwitchCurrentActionMap(inputEnabled ? "Player" : "Menu");
    }

    public void ControlsChanged()
    {
        Debug.Log("Controls changed to " + playerInput.currentControlScheme);
    }
}
