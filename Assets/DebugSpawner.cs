using UnityEngine;
using System.Collections;

public class DebugSpawner : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    public GameObject debugPrefab;      // The obstacle prefab
    public GameObject indicatorPrefab;  // Empty GameObject with SpriteRenderer

    [Header("Spawn Timing")]
    public float startSpawnInterval = 4f;
    public float minSpawnInterval = 2f;
    public float startTravelTime = 8f;
    public float minTravelTime = 3f;
    public float difficultyRampTime = 120f;

    [Header("Button Sprites")]
    public Sprite topLeftSprite;        // L2
    public Sprite topMiddleSprite;      // Triangle
    public Sprite topRightSprite;       // R2
    public Sprite middleLeftSprite;     // Square
    public Sprite middleMiddleLeft;     // L2 (combo)
    public Sprite middleMiddleRight;    // L3 (combo)
    public Sprite middleRightSprite;    // Circle
    public Sprite bottomLeftSprite;     // L1
    public Sprite bottomMiddleSprite;   // X
    public Sprite bottomRightSprite;    // R1

    private float elapsedTime = 0f;

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float currentSpawnInterval = GetCurrentSpawnInterval();
            float currentTravelTime = GetCurrentTravelTime();

            SpawnOneRandom(currentTravelTime);

            elapsedTime += currentSpawnInterval;
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void SpawnOneRandom(float travelTime)
    {
        int zone = Random.Range(0, gridManager.rows * gridManager.cols);
        Vector3 spawn = gridManager.GetSpawnPositionInCell(zone);
        Vector3 arrival = gridManager.GetArrivalPositionInCell(zone);

        if (debugPrefab != null && indicatorPrefab != null)
        {
            GameObject go = Instantiate(debugPrefab, spawn, Quaternion.identity);
            var obstacle = go.AddComponent<ObstacleRhythmTarget>();

            // Collect sprites for this cell
            Sprite[] spritesForCell = GetSpritesForCell(zone);

            // Initialize obstacle with indicator prefab and sprites
            obstacle.Init(spawn, arrival, zone, travelTime, indicatorPrefab, spritesForCell);
        }
    }

    private Sprite[] GetSpritesForCell(int cellIndex)
    {
        switch (cellIndex)
        {
            case 0: return new Sprite[] { topLeftSprite };
            case 1: return new Sprite[] { topMiddleSprite };
            case 2: return new Sprite[] { topRightSprite };
            case 3: return new Sprite[] { middleLeftSprite };
            case 4: return new Sprite[] { middleMiddleLeft, middleMiddleRight }; // Combo
            case 5: return new Sprite[] { middleRightSprite };
            case 6: return new Sprite[] { bottomLeftSprite };
            case 7: return new Sprite[] { bottomMiddleSprite };
            case 8: return new Sprite[] { bottomRightSprite };
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
}
