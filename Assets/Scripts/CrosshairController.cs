using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock; // For DualShock/DualSense
using UnityEngine.InputSystem.Utilities;

public class CrosshairController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector2 moveInput = Vector2.zero;

    private DualSenseGamepadHID dualSenseGamepad;

    void Start()
    {
        // Get DualSense controller if connected
        if (Gamepad.current is DualSenseGamepadHID ds)
        {
            dualSenseGamepad = ds;
        }
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleControllerInput();

        // Move crosshair based on input
        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }

    void HandleKeyboardInput()
    {
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;  // Up
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;  // Down
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;  // Left (will override controller Left+Right input)
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;  // Right
    }

    void HandleControllerInput()
    {
        if (dualSenseGamepad == null) return;

        // Move Up/Down with L2 (descend) and R2 (ascend)
        float ascend = dualSenseGamepad.rightTrigger.ReadValue(); // R2
        float descend = dualSenseGamepad.leftTrigger.ReadValue(); // L2

        moveInput.y += ascend > 0.1f ? 1 : 0;
        moveInput.y -= descend > 0.1f ? 1 : 0;

        // Move Left/Right only if both L1 and R1 pressed
        bool leftShoulder = dualSenseGamepad.leftShoulder.isPressed;  // L1
        bool rightShoulder = dualSenseGamepad.rightShoulder.isPressed; // R1

        if (leftShoulder && rightShoulder)
        {
            // For example, toggle left with left stick or something here
            // Since L1+R1 are buttons, no axis. We'll simulate by using left stick horizontal:
            float leftStickX = dualSenseGamepad.leftStick.x.ReadValue();

            if (leftStickX < -0.1f)
                moveInput.x -= 1;
            else if (leftStickX > 0.1f)
                moveInput.x += 1;
        }
    }
}
