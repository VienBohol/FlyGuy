using UnityEngine;
using System.Collections;

public class DebugSpawner : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    public GameObject debugPrefab;

    [Header("Spawn Timing")]
    public float startSpawnInterval = 4f;   // starting interval between obstacles
    public float minSpawnInterval = 2f;     // fastest interval
    public float startTravelTime = 8f;      // initial travel time
    public float minTravelTime = 3f;        // fastest travel time
    public float difficultyRampTime = 120f; // seconds to reach max difficulty

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

        if (debugPrefab != null)
        {
            GameObject go = Instantiate(debugPrefab, spawn, Quaternion.identity);
            var obstacle = go.AddComponent<ObstacleRhythmTarget>();
            obstacle.Init(spawn, arrival, zone, travelTime);
        }
    }

    // Smoothly scale spawn interval from start → min
    private float GetCurrentSpawnInterval()
    {
        float t = Mathf.Clamp01(elapsedTime / difficultyRampTime);
        // Quadratic curve for smoother ramp-up: easy start, faster later
        return Mathf.Lerp(startSpawnInterval, minSpawnInterval, t * t);
    }

    // Smoothly scale travel time from start → min
    private float GetCurrentTravelTime()
    {
        float t = Mathf.Clamp01(elapsedTime / difficultyRampTime);
        return Mathf.Lerp(startTravelTime, minTravelTime, t * t);
    }
}
