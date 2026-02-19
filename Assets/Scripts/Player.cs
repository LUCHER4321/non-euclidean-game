using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    public float moveSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpHeight = 0.5f;
    public bool isRunning = false;
    public PlayerInput playerInput;

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
        float speed = isRunning ? runSpeed : moveSpeed;
        Vector2 velocity = input.normalized * speed;
        Move(velocity);
    }

    public void PlayerJump()
    {
        if (!inputEnabled) return;
        Jump(jumpHeight);
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
