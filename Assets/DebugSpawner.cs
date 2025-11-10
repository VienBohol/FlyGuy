using UnityEngine;
using System.Collections;
using System;

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
    public float minSpawnInterval = 2f;
    public float startTravelTime = 8f;
    public float minTravelTime = 3f;
    public float difficultyRampTime = 120f;

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

            SpawnOneRandom(travelTime);

            elapsedTime += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnOneRandom(float travelTime)
    {
        int zone = UnityEngine.Random.Range(0, gridManager.rows * gridManager.cols);
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

    private float GetCurrentSpawnInterval()
    {
        float t = Mathf.Clamp01(elapsedTime / difficultyRampTime);
        return Mathf.Lerp(startSpawnInterval, minSpawnInterval, t * t);
    }

    private float GetCurrentTravelTime()
    {
        float t = Mathf.Clamp01(elapsedTime / difficultyRampTime);
        return Mathf.Lerp(startTravelTime, minTravelTime, t * t);
    }

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
