using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class DestroyableObject : MonoBehaviour
{
    public enum ShapeButton { Triangle, Square, X, Circle }
    public ShapeButton requiredButton = ShapeButton.Triangle;

    private bool isHovered = false;

    // Reference to the child GameObject with the sprite animation
    public GameObject hoverAnimationObject;

    // Optional: keep color-changing ability for fallback or debugging
    private Renderer objectRenderer;
    private Color originalColor;
    public Color hoverColor = Color.yellow; // Not strictly needed now if using animation

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
            originalColor = objectRenderer.material.color;

        // Ensure hover animation is hidden at start
        if (hoverAnimationObject != null)
            hoverAnimationObject.SetActive(false);
    }

    public void SetHovered(bool hovered)
    {
        isHovered = hovered;

        // Toggle the hover animation
        if (hoverAnimationObject != null)
            hoverAnimationObject.SetActive(isHovered);

        // Optional: If you want to keep color change for fallback/testing
        if (objectRenderer != null)
        objectRenderer.material.color = isHovered ? hoverColor : originalColor;
    }

    public bool IsCorrectShapeButtonPressed()
    {
        if (Gamepad.current == null) return false;

        switch (requiredButton)
        {
            case ShapeButton.Triangle:
                return Gamepad.current.buttonNorth.wasPressedThisFrame;
            case ShapeButton.Square:
                return Gamepad.current.buttonWest.wasPressedThisFrame;
            case ShapeButton.X:
                return Gamepad.current.buttonSouth.wasPressedThisFrame;
            case ShapeButton.Circle:
                return Gamepad.current.buttonEast.wasPressedThisFrame;
            default:
                return false;
        }
    }

    void DestroyObject()
    {
        // Haptic feedback for DualSense controller (PC)
        var dualSense = Gamepad.current as DualSenseGamepadHID;
        if (dualSense != null)
        {
            // Set both motors for rumble (max intensity, 0.3 seconds duration)
            dualSense.SetMotorSpeeds(1.0f, 1.0f);
            Invoke(nameof(StopHaptics), 0.3f); // stop rumble after short duration
        }
        // Destroy the object as usual
        Destroy(gameObject);
    }

    void StopHaptics()
    {
        var dualSense = Gamepad.current as DualSenseGamepadHID;
        if (dualSense != null)
        {
            dualSense.SetMotorSpeeds(0.0f, 0.0f);
        }
    }
}
