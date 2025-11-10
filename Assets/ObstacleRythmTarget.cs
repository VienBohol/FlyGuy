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
    [Range(0f,1f)] public float hitVibrationStrength = 0.2f;
    [Range(0f,1f)] public float missVibrationStrength = 0.3f;
    public float vibrationDuration = 0.15f;

    private Vector3 startPos;
    private Vector3 endPos;
    private float travelTime;
    private int cellIndex;
    private bool hittable = false;
    private bool wasHit = false;
    private float arrivalTime;

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

            // Make obstacle hittable
            bool shouldBeHittable = hitEvaluator.IsWithinWindow(arrivalTime - Time.time);
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
            TriggerHitVibration();
            Destroy(gameObject);
        }
    }

    private void OnMiss()
    {
        if (!wasHit)
        {
            wasHit = true; // prevent double triggers
            spawner.SpawnHitFeedback("Miss", transform.position);
            OnPlayerMiss?.Invoke();
            TriggerMissVibration();
        }
    }

    private void TriggerHitVibration()
    {
        if (dualSenseRumble == null) return;

        dualSenseRumble.LeftRumble = hitVibrationStrength;
        dualSenseRumble.RightRumble = hitVibrationStrength;

        StartCoroutine(StopVibration(vibrationDuration));
    }

    private void TriggerMissVibration()
    {
        if (dualSenseRumble == null) return;

        dualSenseRumble.LeftRumble = missVibrationStrength;
        dualSenseRumble.RightRumble = missVibrationStrength;

        StartCoroutine(StopVibration(vibrationDuration));
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
