using JetBrains.Annotations;
using UnityEngine;

public class CameraAndArmsController : MonoBehaviour
{
    [Header("References")]
    public PlayerRhythmInput playerInput;   // reference to get CurrentSelectedCell
    public Transform crosshairTransform;
    public Transform armsTransform;
    public Camera mainCamera;
    public float cornerMultiplier = 1.1f; // tweak in inspector if needed

    [Header("Tilt Settings")]
    public float leftTilt = 15f;     // arms tilt when targeting left
    public float rightTilt = -15f;   // arms tilt when targeting right
    public float upTilt = 5f;        // camera tilt when targeting top row
    public float downTilt = -5f;     // camera tilt when targeting bottom row
    public float tiltSmoothSpeed = 8f; 

    private Vector3 initialArmsRotation;

    private void Start()
    {
        if (armsTransform != null)
            initialArmsRotation = armsTransform.localEulerAngles;

        // Initialize camera rotation neutral
        if (mainCamera != null)
            mainCamera.transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        UpdateCrosshairPosition();
        UpdateArmsAndCameraTilt();
    }

    private void UpdateCrosshairPosition()
    {
        if (crosshairTransform == null || playerInput == null || playerInput.gridManager == null)
            return;

        int cell = playerInput.CurrentSelectedCell;
        Vector3[,] grid = playerInput.gridManager.GetArrivalPoints2D();
        int row = cell / 3;
        int col = cell % 3;

        Vector3 targetPos = grid[row, col];
        crosshairTransform.position = Vector3.Lerp(crosshairTransform.position, targetPos, Time.deltaTime * 15f);
    }

    private void UpdateArmsAndCameraTilt()
    {
        if (armsTransform == null || mainCamera == null || playerInput == null)
            return;

        int cell = playerInput.CurrentSelectedCell;
        int row = cell / 3; // 0 = top, 1 = middle, 2 = bottom
        int col = cell % 3; // 0 = left, 1 = middle, 2 = right
        

        // Horizontal tilt (Z) for arms
        float targetZ = 0f;
        if (col == 0) targetZ = leftTilt;
        else if (col == 2) targetZ = rightTilt;

        // Smooth arms rotation
        Quaternion armsTarget = Quaternion.Euler(0f, 0f, targetZ);
        armsTransform.localRotation = Quaternion.Lerp(armsTransform.localRotation, armsTarget, Time.deltaTime * tiltSmoothSpeed);

        // Vertical tilt (X) for camera based on row
        float targetX = 0f;
        if (row == 0) targetX = -upTilt;     // top row tilts camera slightly down
        else if (row == 2) targetX = -downTilt;
        // Corner multiplier

        
        if ((row == 0 || row == 2) && (col == 0 || col == 2))
        {
            targetZ *= cornerMultiplier; // horizontal tilt
            targetX *= cornerMultiplier; // vertical tilt
        }



        // Combine horizontal and vertical tilt for camera
        Quaternion camTarget = Quaternion.Euler(targetX, 0f, targetZ * 0.5f);
        mainCamera.transform.localRotation = Quaternion.Lerp(mainCamera.transform.localRotation, camTarget, Time.deltaTime * tiltSmoothSpeed);
    }
}
