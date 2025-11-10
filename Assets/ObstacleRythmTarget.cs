using UnityEngine;
using System;
using System.Collections;
using UniSense;
using DualSenseSample.Inputs;

public class ObstacleRhythmTarget : MonoBehaviour
{
    [Header("References")]
    public DebugSpawner spawner;
    public Renderer rend;
    public HitEvaluator hitEvaluator = new HitEvaluator();
    public DualSenseRumble dualSenseRumble; // assigned automatically

    [Header("Vibration Settings")]
    [Range(0f,1f)] public float approachVibrationStrength = 0.2f;
    [Range(0f,1f)] public float missVibrationStrength = 0.3f;
    public float approachVibrationDuration = 0.2f;
    public float missVibrationDuration = 0.15f;

    private Vector3 startPos;
    private Vector3 endPos;
    private float travelTime;
    private int cellIndex;
    private bool hittable = false;
    private bool wasHit = false;
    private float arrivalTime;
    private bool approachTriggered = false;

    public static event Action OnPlayerMiss;

    private void OnEnable()
    {
        PlayerRhythmInput.OnCellInput += TryHandleHit;
    }

    private void OnDisable()
    {
        PlayerRhythmInput.OnCellInput -= TryHandleHit;
    }

    public void Init(Vector3 spawn, Vector3 arrival, int zone, float travel)
    {
        startPos = spawn;
        endPos = arrival;
        cellIndex = zone;
        travelTime = travel;

        // automatically find DualSenseRumble if null
        if (dualSenseRumble == null)
        {
            dualSenseRumble = FindFirstObjectByType<DualSenseRumble>();
            if (dualSenseRumble == null)
                Debug.LogWarning("No DualSenseRumble found in scene!");
        }

        StartCoroutine(TravelRoutine());
    }

    public Renderer GetRenderer()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        return rend;
    }

    private IEnumerator TravelRoutine()
    {
        float elapsed = 0f;
        arrivalTime = Time.time + travelTime;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            transform.position = Vector3.Lerp(startPos, endPos, t);

            float timeUntilArrival = arrivalTime - Time.time;

            // Make obstacle hittable
            bool shouldBeHittable = hitEvaluator.IsWithinWindow(timeUntilArrival);
            if (shouldBeHittable && !hittable)
            {
                hittable = true;
                spawner.SetObstacleMaterial(this, spawner.hittableMaterial);
            }
            else if (!shouldBeHittable && hittable)
            {
                hittable = false;
                spawner.SetObstacleMaterial(this, spawner.defaultMaterial);
            }

            // Approach vibration trigger (after 50% travel)
            if (!approachTriggered && elapsed > travelTime * 0.5f)
            {
                TriggerApproachVibration();
                approachTriggered = true;
            }

            yield return null;
        }

        // Miss if not hit
        if (!wasHit)
            OnMiss();

        Destroy(gameObject);
    }

    private void TryHandleHit(int inputCell, int aimCell)
    {
        if (wasHit || !hittable) return;
        if (inputCell != cellIndex || aimCell != cellIndex) return;

        float timeUntilArrival = arrivalTime - Time.time;
        string result = hitEvaluator.EvaluateHit(timeUntilArrival);

        if (result != "Miss")
        {
            wasHit = true;
            spawner.SpawnHitFeedback(result, transform.position);
            Destroy(gameObject); // no vibration on hit
        }
    }

    private void OnMiss()
    {
        if (!wasHit)
        {
            spawner.SpawnHitFeedback("Miss", transform.position);
            OnPlayerMiss?.Invoke();
            TriggerMissVibration();
        }
    }

    private void TriggerApproachVibration()
    {
        if (dualSenseRumble == null) return;

        float left = 0f, right = 0f;

        // Column mapping: 0=left, 1=middle, 2=right
        int column = cellIndex % 3;
        if (column == 0) left = approachVibrationStrength;
        else if (column == 2) right = approachVibrationStrength;
        else left = right = approachVibrationStrength;

        dualSenseRumble.LeftRumble = left;
        dualSenseRumble.RightRumble = right;

        StartCoroutine(StopVibration(approachVibrationDuration));
    }

    private void TriggerMissVibration()
    {
        if (dualSenseRumble == null) return;

        dualSenseRumble.LeftRumble = missVibrationStrength;
        dualSenseRumble.RightRumble = missVibrationStrength;

        StartCoroutine(StopVibration(missVibrationDuration));
    }

    private IEnumerator StopVibration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (dualSenseRumble != null)
        {
            dualSenseRumble.LeftRumble = 0f;
            dualSenseRumble.RightRumble = 0f;
        }
    }
}
