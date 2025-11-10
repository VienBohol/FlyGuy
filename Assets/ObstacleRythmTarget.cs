using UnityEngine;
using System;
using System.Collections;

public class ObstacleRhythmTarget : MonoBehaviour
{
    [Header("References")]
    public DebugSpawner spawner;
    public Renderer rend;
    public HitEvaluator hitEvaluator = new HitEvaluator();

    private Vector3 startPos;
    private Vector3 endPos;
    private float travelTime;
    private int cellIndex;
    private bool hittable = false;
    private bool wasHit = false;
    private float arrivalTime;

    // New event for when a miss happens
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

            yield return null;
        }

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
            Destroy(gameObject);
        }
    }

    private void OnMiss()
    {
        if (!wasHit)
        {
            spawner.SpawnHitFeedback("Miss", transform.position);
            OnPlayerMiss?.Invoke(); // Notify listeners
        }
    }
}
