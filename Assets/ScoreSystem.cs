using UnityEngine;
using TMPro; // For text UI
using System.Collections.Generic;

public class ScoreSystem : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI scoreText; // Assign in inspector

    [Header("Scoring Values")]
    public int perfectPoints = 250;
    public int greatPoints = 150;
    public int goodPoints = 100;
    public int badPoints = 50;
    public int comboBonus = 20; // Added per combo streak

    private int totalScore = 0;
    private int comboCount = 0;

    private static ScoreSystem instance;
    public static ScoreSystem Instance => instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void OnEnable()
    {
        ObstacleRhythmTarget.OnPlayerMiss += HandleMiss;
        DebugSpawner.OnFeedbackEvent += HandleHitResult;
    }

    private void OnDisable()
    {
        ObstacleRhythmTarget.OnPlayerMiss -= HandleMiss;
        DebugSpawner.OnFeedbackEvent -= HandleHitResult;
    }

    private void HandleHitResult(string result)
    {
        int basePoints = result switch
        {
            "Perfect" => perfectPoints,
            "Great" => greatPoints,
            "Good" => goodPoints,
            "Bad" => badPoints,
            _ => 0
        };

        if (basePoints > 0)
        {
            comboCount++;
            int bonus = comboCount * comboBonus;
            totalScore += basePoints + bonus;
        }
        else
        {
            HandleMiss();
        }

        UpdateUI();
    }

    private void HandleMiss()
    {
        comboCount = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {totalScore}\nCombo: {comboCount}";
    }
}
