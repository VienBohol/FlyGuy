using UnityEngine;

public class CameraSnapController : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    [Tooltip("The focus target that moves between arrival points (place your crosshair here).")]
    public Transform focusTarget;

    [Header("Input / Movement")]
    public float deadzone = 0.2f;        // ignore tiny stick input
    public float inputThreshold = 0.33f; // axis threshold for deciding left/center/right / up/center/down
    public float snapSpeed = 15f;        // how quickly the focus target moves to the selected arrival point

    [Header("Camera Follow")]
    public Vector3 cameraOffset = new Vector3(0f, 0f, -8f); // camera stays this offset from the focus target
    public float cameraFollowSpeed = 8f;    // how smoothly the camera follows the focus target

    // runtime
    private PlayerControls controls;
    private Vector2 leftStickInput;

    private Vector3[,] gridPoints;   // [row, col] (row 0 = top)
    private int currentRow = 1;      // middle row
    private int currentCol = 1;      // middle col

    private void Awake()
    {
        controls = new PlayerControls();

        if (gridManager != null)
        {
            // Ensure grid is computed and grab positions
            gridManager.RecomputeGrid();
            gridPoints = gridManager.GetArrivalPoints2D();
        }
        else
        {
            Debug.LogError("CameraSnapController: Missing GridManager reference!");
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void Start()
    {
        // make sure we have grid points
        if (gridManager != null && gridPoints == null)
            gridPoints = gridManager.GetArrivalPoints2D();

        // ensure focusTarget exists; if not, create a temporary one as child
        if (focusTarget == null)
        {
            GameObject ft = new GameObject("FocusTarget");
            focusTarget = ft.transform;
            focusTarget.SetParent(null); // keep it in world space
        }

        // place focus target and camera at center start
        Vector3 centerPos = gridPoints != null ? gridPoints[1, 1] : transform.position;
        focusTarget.position = centerPos;

        // initialize camera so it sits at proper offset behind the focus target
        transform.position = focusTarget.position + cameraOffset;
    }

    private void Update()
    {
        ReadJoystick();
        UpdateGridSelection();
        MoveFocusTarget();
        CameraFollow();
    }

    private void ReadJoystick()
    {
        leftStickInput = controls.Player.Move.ReadValue<Vector2>();

        // apply deadzone
        if (leftStickInput.magnitude < deadzone)
            leftStickInput = Vector2.zero;
    }

    private void UpdateGridSelection()
    {
        if (gridPoints == null) return;

        // Default is center
        int row = 1;
        int col = 1;

        // If joystick outside deadzone, choose cell based on axis thresholds
        if (leftStickInput != Vector2.zero)
        {
            // Horizontal: left (-) -> col 0, center -> 1, right (+) -> 2
            if (leftStickInput.x < -inputThreshold) col = 0;
            else if (leftStickInput.x > inputThreshold) col = 2;
            else col = 1;

            // Vertical: remember gridPoints row 0 is top, so up (+y) -> row 0
            if (leftStickInput.y > inputThreshold) row = 0;
            else if (leftStickInput.y < -inputThreshold) row = 2;
            else row = 1;
        }
        else
        {
            // stick released -> recenter
            row = 1;
            col = 1;
        }

        // apply
        currentRow = Mathf.Clamp(row, 0, gridPoints.GetLength(0) - 1);
        currentCol = Mathf.Clamp(col, 0, gridPoints.GetLength(1) - 1);
    }

    private void MoveFocusTarget()
    {
        if (gridPoints == null || focusTarget == null) return;

        Vector3 targetPos = gridPoints[currentRow, currentCol];
        // Smoothly move the focus (crosshair) to the chosen arrival position
        focusTarget.position = Vector3.Lerp(focusTarget.position, targetPos, Time.deltaTime * snapSpeed);

        // Optional: snap when very close to avoid small floating
        if ((focusTarget.position - targetPos).sqrMagnitude < 0.0001f)
            focusTarget.position = targetPos;
    }

    private void CameraFollow()
    {
        if (focusTarget == null) return;

        Vector3 desired = focusTarget.position + cameraOffset;
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * cameraFollowSpeed);

        // Optional: snap when very close to avoid micro-oscillation
        if ((transform.position - desired).sqrMagnitude < 0.0001f)
            transform.position = desired;
    }
}
