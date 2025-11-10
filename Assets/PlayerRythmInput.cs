using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRhythmInput : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;

    private InputSystem_Actions controls;
    private Dictionary<string, int> inputToCell = new Dictionary<string, int>();

    public delegate void CellInputEvent(int inputCell, int aimCell);
    public static event CellInputEvent OnCellInput;

    public int CurrentSelectedCell { get; private set; } = 4; // Middle default

    // Combo timing
    private float l3PressedTime;
    private float r3PressedTime;
    private const float comboWindow = 0.15f;

    private void Awake()
    {
        controls = new InputSystem_Actions();
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

    private void Update()
    {
        Vector2 leftStick = controls.Player.Move.ReadValue<Vector2>();
        if (leftStick.magnitude < 0.2f) leftStick = Vector2.zero;

        int row = 1, col = 1;
        if (leftStick != Vector2.zero)
        {
            col = leftStick.x < -0.33f ? 0 : (leftStick.x > 0.33f ? 2 : 1);
            row = leftStick.y > 0.33f ? 0 : (leftStick.y < -0.33f ? 2 : 1);
        }

        CurrentSelectedCell = row * 3 + col;
    }

    private void BuildInputMapping()
    {
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
            controls.Player.L1.performed += ctx => HandleInput("L1");
            controls.Player.L2.performed += ctx => HandleInput("L2");
            controls.Player.R1.performed += ctx => HandleInput("R1");
            controls.Player.R2.performed += ctx => HandleInput("R2");
            controls.Player.Triangle.performed += ctx => HandleInput("Triangle");
            controls.Player.Square.performed += ctx => HandleInput("Square");
            controls.Player.Circle.performed += ctx => HandleInput("Circle");
            controls.Player.Cross.performed += ctx => HandleInput("Cross");
            controls.Player.L3.performed += ctx => CheckL3R3Combo("L3");
            controls.Player.R3.performed += ctx => CheckL3R3Combo("R3");
        }
        else
        {
            controls.Player.L1.performed -= ctx => HandleInput("L1");
            controls.Player.L2.performed -= ctx => HandleInput("L2");
            controls.Player.R1.performed -= ctx => HandleInput("R1");
            controls.Player.R2.performed -= ctx => HandleInput("R2");
            controls.Player.Triangle.performed -= ctx => HandleInput("Triangle");
            controls.Player.Square.performed -= ctx => HandleInput("Square");
            controls.Player.Circle.performed -= ctx => HandleInput("Circle");
            controls.Player.Cross.performed -= ctx => HandleInput("Cross");
            controls.Player.L3.performed -= ctx => CheckL3R3Combo("L3");
            controls.Player.R3.performed -= ctx => CheckL3R3Combo("R3");
        }
    }

    private void CheckL3R3Combo(string key)
    {
        float now = Time.time;
        if (key == "L3")
        {
            if (now - r3PressedTime < comboWindow) HandleInput("L3R3");
            l3PressedTime = now;
        }
        else
        {
            if (now - l3PressedTime < comboWindow) HandleInput("L3R3");
            r3PressedTime = now;
        }
    }

    private void HandleInput(string key)
    {
        if (!inputToCell.TryGetValue(key, out int inputCell)) return;

        // Invoke event
        OnCellInput?.Invoke(inputCell, CurrentSelectedCell);

        // Only log once per press
        Debug.Log($"Pressed {key} | InputCell: {inputCell}, AimCell: {CurrentSelectedCell}");
    }
}
