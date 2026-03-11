using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    [Header("Player")]
    public PlayerInput playerInput;
    [Min(0f)]
    public float sensitivity = 0.5f;
    public bool inputEnabled = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
        Vector2 input = context.ReadValue<Vector2>();
        input *= sensitivity;
        Look(input);
    }

    public void ToggleInput()
    {
        inputEnabled = !inputEnabled;
        Cursor.visible = !inputEnabled;
        Cursor.lockState = inputEnabled ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void ControlsChanged(PlayerInput pi)
    {
        Debug.Log("Controls changed to " + pi.currentControlScheme);
    }
}
