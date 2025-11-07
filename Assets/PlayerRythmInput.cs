using UnityEngine;
using System.Collections.Generic;

public class PlayerRhythmInput : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    private PlayerControls controls;
    private Dictionary<string, int> inputToCell = new Dictionary<string, int>();

    public delegate void CellInputEvent(int cellIndex);
    public static event CellInputEvent OnCellInput; // Fired when player presses a rhythm button
    public int CurrentSelectedCell { get; private set; } = 4; // middle by default
    private void Update()
{
    Vector2 leftStick = controls.Player.Move.ReadValue<Vector2>();
    // Apply deadzone
    if (leftStick.magnitude < 0.2f) leftStick = Vector2.zero;

    int row = 1, col = 1;
    if (leftStick != Vector2.zero)
    {
        col = leftStick.x < -0.33f ? 0 : (leftStick.x > 0.33f ? 2 : 1);
        row = leftStick.y > 0.33f ? 0 : (leftStick.y < -0.33f ? 2 : 1);
    }
    CurrentSelectedCell = row * 3 + col;
}

    private void Awake()
    {
        controls = new PlayerControls();
        BuildInputMapping();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        RegisterCallbacks(true);
    }

    private void OnDisable()
    {
        RegisterCallbacks(false);
        controls.Player.Disable();
    }

    private void BuildInputMapping()
    {
        // 3×3 grid index layout:
        //  0 1 2
        //  3 4 5
        //  6 7 8
        //
        // Example mapping:
        // Top row: L2, Triangle, R2
        // Middle row: Square, L3+R3, Circle
        // Bottom row: L1, Cross, R1

        inputToCell["L2"] = 0;
        inputToCell["Triangle"] = 1;
        inputToCell["R2"] = 2;
        inputToCell["Square"] = 3;
        inputToCell["L3R3"] = 4;
        inputToCell["Circle"] = 5;
        inputToCell["L1"] = 6;
        inputToCell["Cross"] = 7;
        inputToCell["R1"] = 8;
    }

    private void RegisterCallbacks(bool enable)
    {
        if (enable)
        {
            controls.Player.L1.performed += _ => HandleInput("L1");
            controls.Player.L2.performed += _ => HandleInput("L2");
            controls.Player.R1.performed += _ => HandleInput("R1");
            controls.Player.R2.performed += _ => HandleInput("R2");
            controls.Player.Triangle.performed += _ => HandleInput("Triangle");
            controls.Player.Square.performed += _ => HandleInput("Square");
            controls.Player.Circle.performed += _ => HandleInput("Circle");
            controls.Player.Cross.performed += _ => HandleInput("Cross");
            controls.Player.L3.performed += _ => CheckL3R3Combo();
            controls.Player.R3.performed += _ => CheckL3R3Combo();
        }
        else
        {
            controls.Player.L1.performed -= _ => HandleInput("L1");
            controls.Player.L2.performed -= _ => HandleInput("L2");
            controls.Player.R1.performed -= _ => HandleInput("R1");
            controls.Player.R2.performed -= _ => HandleInput("R2");
            controls.Player.Triangle.performed -= _ => HandleInput("Triangle");
            controls.Player.Square.performed -= _ => HandleInput("Square");
            controls.Player.Circle.performed -= _ => HandleInput("Circle");
            controls.Player.Cross.performed -= _ => HandleInput("Cross");
            controls.Player.L3.performed -= _ => CheckL3R3Combo();
            controls.Player.R3.performed -= _ => CheckL3R3Combo();
        }
    }

    private float l3PressedTime;
    private float r3PressedTime;
    private const float comboWindow = 0.15f; // must be pressed within 150ms

    private void CheckL3R3Combo()
    {
        float now = Time.time;
        if (now - l3PressedTime < comboWindow || now - r3PressedTime < comboWindow)
        {
            HandleInput("L3R3");
        }
        l3PressedTime = now;
        r3PressedTime = now;
    }

    private void HandleInput(string key)
    {
        if (!inputToCell.TryGetValue(key, out int cellIndex))
            return;

        OnCellInput?.Invoke(cellIndex);
        // Later we’ll check for hit detection here
    }
}
