using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairMover : MonoBehaviour
{
    public float moveSpeed = 500f;
    public Camera followCamera; // Assign camera in Inspector
    public float cameraFollowSpeed = 5f;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            Debug.LogError("CrosshairMover: Must be attached to a UI object inside a Canvas.");

        if (followCamera == null)
            followCamera = Camera.main;
    }

    void Update()
    {
        MoveCrosshair();
        MoveCamera();
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

            if (Gamepad.current.leftShoulder.isPressed && !Gamepad.current.rightShoulder.isPressed)
                move.x -= 1;
            else if (Gamepad.current.rightShoulder.isPressed && !Gamepad.current.leftShoulder.isPressed)
                move.x += 1;
        }

        rectTransform.anchoredPosition += move * moveSpeed * Time.deltaTime;
    }

    void MoveCamera()
    {
        if (followCamera == null || rectTransform == null)
            return;

        // Convert crosshair UI position (world space) to screen space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(followCamera, rectTransform.position);

        // Convert screen point to world point at the camera's near clip plane + some offset (camera forward)
        Vector3 targetPos = followCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, followCamera.nearClipPlane + 1f));

        // Keep camera's original Z position to avoid moving too close or far
        targetPos.z = followCamera.transform.position.z;

        // Smoothly move camera toward new position
        followCamera.transform.position = Vector3.Lerp(followCamera.transform.position, targetPos, cameraFollowSpeed * Time.deltaTime);
    }
}
