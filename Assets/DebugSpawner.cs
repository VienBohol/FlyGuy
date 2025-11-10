using UnityEngine;
using System;
using System.Collections;
using DualSenseSample.Inputs;

public class DebugSpawner : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    public GameObject obstaclePrefab;
    public GameObject indicatorPrefab;

    [Header("Feedback System")]
    public SpriteRenderer feedbackRenderer; // assign the sprite renderer childed to the crosshair

    [Header("Feedback Sprites")]
    public Sprite perfectSprite;
    public Sprite greatSprite;
    public Sprite goodSprite;
    public Sprite badSprite;
    public Sprite missSprite;
    public static event Action<string> OnFeedbackEvent;

    [Header("Button Sprites")]
    public Sprite topLeftSprite;
    public Sprite topMiddleSprite;
    public Sprite topRightSprite;
    public Sprite middleLeftSprite;
    public Sprite middleMiddleLeft;
    public Sprite middleMiddleRight;
    public Sprite middleRightSprite;
    public Sprite bottomLeftSprite;
    public Sprite bottomMiddleSprite;
    public Sprite bottomRightSprite;

    [Header("Materials")]
    public Material defaultMaterial;
    public Material hittableMaterial;

    [Header("Spawn Timing")]
    public float startSpawnInterval = 4f;
    public float minSpawnInterval = 1.2f;  // minimum spawn interval
    public float startTravelTime = 8f;
    public float minTravelTime = 4.5f;    // fastest travel time
    public float difficultyRampTime = 180f;

    [Header("Burst Settings")]
    [Range(0f, 1f)] public float startBurstChance = 0.1f; // initial chance for burst
    [Range(0f, 1f)] public float maxBurstChance = 0.5f;   // max chance at endgame
    public float burstStagger = 0.3f; // seconds between obstacles in burst

    private float elapsedTime = 0f;
    private Coroutine feedbackRoutine;

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        if (feedbackRenderer != null)
            feedbackRenderer.sprite = null;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float interval = GetCurrentSpawnInterval();
            float travelTime = GetCurrentTravelTime();

            // Determine if this should be a burst
            float burstChance = Mathf.Lerp(startBurstChance, maxBurstChance, Mathf.Clamp01(elapsedTime / difficultyRampTime));
            if (UnityEngine.Random.value < burstChance)
            {
                SpawnBurst(travelTime);
            }
            else
            {
                SpawnOneRandom(travelTime);
            }

            elapsedTime += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    // ----------------------- Spawn Methods -----------------------

    private void SpawnBurst(float travelTime)
    {
        int[] availableColumns = { 0, 1, 2 }; // left, middle, right
        ShuffleArray(availableColumns);       // random order

        for (int i = 0; i < availableColumns.Length; i++)
        {
            int column = availableColumns[i];
            StartCoroutine(SpawnBurstObstacleDelayed(column, travelTime, i * burstStagger));
        }
    }

    private IEnumerator SpawnBurstObstacleDelayed(int column, float travelTime, float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnOneRandom(travelTime, column);
    }

    private void SpawnOneRandom(float travelTime, int? forcedColumn = null)
    {
        int zone;
        if (forcedColumn.HasValue)
        {
            int row = UnityEngine.Random.Range(0, gridManager.rows);
            zone = row * 3 + forcedColumn.Value;
        }
        else
        {
            zone = UnityEngine.Random.Range(0, gridManager.rows * gridManager.cols);
        }

        Vector3 spawn = gridManager.GetSpawnPositionInCell(zone);
        Vector3 arrival = gridManager.GetArrivalPositionInCell(zone);

        if (obstaclePrefab != null)
        {
            GameObject go = Instantiate(obstaclePrefab, spawn, Quaternion.identity);
            var obstacle = go.AddComponent<ObstacleRhythmTarget>();

            obstacle.spawner = this;
            obstacle.Init(spawn, arrival, zone, travelTime);
            SetObstacleMaterial(obstacle, defaultMaterial);
            SpawnButtonSprites(obstacle.transform, zone);

            // Auto-assign DualSenseRumble
            var dualSenseRumble = FindFirstObjectByType<DualSenseRumble>();
            if (dualSenseRumble != null)
                obstacle.dualSenseRumble = dualSenseRumble;
        }
    }

    private void SpawnButtonSprites(Transform obstacleTransform, int cellIndex)
    {
        Sprite[] sprites = GetSpritesForCell(cellIndex);
        if (sprites.Length == 0) return;

        float offsetStep = 0.7f;
        float startOffset = -(sprites.Length - 1) * offsetStep / 2f;

        for (int i = 0; i < sprites.Length; i++)
        {
            GameObject ind = Instantiate(indicatorPrefab, obstacleTransform);
            ind.transform.localPosition = Vector3.up * 1f + Vector3.right * (startOffset + i * offsetStep);
            var sr = ind.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = sprites[i];
        }
    }

    // ----------------------- Utility -----------------------

    private void ShuffleArray(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    // ----------------------- Difficulty Scaling -----------------------

    private float GetCurrentSpawnInterval()
    {
        float t = Mathf.Clamp01(elapsedTime / difficultyRampTime);
        float interval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, t * t);
        // Optionally factor in GameSpeedManager
        if (GameSpeedManager.Instance != null)
            interval /= GameSpeedManager.Instance.currentSpeed;
        return interval;
    }

    private float GetCurrentTravelTime()
    {
        float t = Mathf.Clamp01(elapsedTime / difficultyRampTime);
        float travel = Mathf.Lerp(startTravelTime, minTravelTime, t * t);
        // Optionally factor in GameSpeedManager
        if (GameSpeedManager.Instance != null)
            travel /= GameSpeedManager.Instance.currentSpeed;
        return travel;
    }

    public Sprite[] GetSpritesForCell(int cellIndex)
    {
        switch (cellIndex)
        {
            case 0: return new[] { topLeftSprite };
            case 1: return new[] { topMiddleSprite };
            case 2: return new[] { topRightSprite };
            case 3: return new[] { middleLeftSprite };
            case 4: return new[] { middleMiddleLeft, middleMiddleRight };
            case 5: return new[] { middleRightSprite };
            case 6: return new[] { bottomLeftSprite };
            case 7: return new[] { bottomMiddleSprite };
            case 8: return new[] { bottomRightSprite };
            default: return new Sprite[0];
        }
    }

    // ----------------------- Materials & Feedback -----------------------

    public void SetObstacleMaterial(ObstacleRhythmTarget obstacle, Material mat)
    {
        if (obstacle != null && mat != null)
        {
            var r = obstacle.GetRenderer();
            if (r != null) r.material = mat;
        }
    }

    public void SpawnHitFeedback(string result, Vector3 unusedPosition)
    {
        if (feedbackRenderer == null) return;

        Sprite sprite = result switch
        {
            "Perfect" => perfectSprite,
            "Great" => greatSprite,
            "Good" => goodSprite,
            "Bad" => badSprite,
            _ => missSprite
        };

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(ShowFeedback(sprite));
        OnFeedbackEvent?.Invoke(result);
    }

    private IEnumerator ShowFeedback(Sprite sprite)
    {
        feedbackRenderer.sprite = sprite;
        yield return new WaitForSeconds(0.8f);
        feedbackRenderer.sprite = null;
    }
}
