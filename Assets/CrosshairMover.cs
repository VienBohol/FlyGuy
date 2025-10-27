using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairMover : MonoBehaviour
{
    public float moveSpeed = 500f;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            Debug.LogError("CrosshairMover: Must be attached to a UI object inside a Canvas.");
    }

    void Update()
    {
        MoveCrosshair();
    }

    void MoveCrosshair()
    {
        if (rectTransform == null)
            return;

        Vector2 move = Vector2.zero;

        if (Gamepad.current != null)
        {
            float r2Value = Gamepad.current.rightTrigger.ReadValue();
            float l2Value = Gamepad.current.leftTrigger.ReadValue();

            if (r2Value > 0.1f)
                move.y += 1;
            if (l2Value > 0.1f)
                move.y -= 1;

            // Move left if L1 held, move right if R1 held
            if (Gamepad.current.leftShoulder.isPressed && !Gamepad.current.rightShoulder.isPressed)
                move.x -= 1;
            else if (Gamepad.current.rightShoulder.isPressed && !Gamepad.current.leftShoulder.isPressed)
                move.x += 1;
            // If both held, you can decide to do nothing, or move in a direction
            // For example, move x = 0 or pick one side as dominant
        }

        rectTransform.anchoredPosition += move * moveSpeed * Time.deltaTime;
    }
}
