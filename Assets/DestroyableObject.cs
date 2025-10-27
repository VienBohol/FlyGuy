using UnityEngine;
using UnityEngine.InputSystem;

public class DestroyableObject : MonoBehaviour
{
    public enum ShapeButton { Triangle, Square, X, Circle }
    public ShapeButton requiredButton = ShapeButton.Triangle;

    private bool isHovered = false;

    public void SetHovered(bool hovered)
    {
        isHovered = hovered;
        // Optional: Change object color or indicator when hovered
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
}

