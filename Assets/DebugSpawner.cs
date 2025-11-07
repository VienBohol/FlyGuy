using UnityEngine;
using System.Collections;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public GridManager gridManager;
    public GameObject obstaclePrefab;

    [Header("Spawn Timing")]
    public float startSpawnInterval = 4f;
    public float minSpawnInterval = 2f;
    public float startTravelTime = 8f;
    public float minTravelTime = 3f;

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
            float speedFactor = GameSpeedManager.Instance.currentSpeed;

            float currentSpawnInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval,
                (speedFactor - 1f) / (GameSpeedManager.Instance.maxSpeed - 1f));

            float currentTravelTime = Mathf.Lerp(startTravelTime, minTravelTime,
                (speedFactor - 1f) / (GameSpeedManager.Instance.maxSpeed - 1f));

            SpawnRandom(currentTravelTime);

            elapsedTime += currentSpawnInterval;
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void SpawnRandom(float travelTime)
    {
        int zone = Random.Range(0, gridManager.rows * gridManager.cols);
        Vector3 spawn = gridManager.GetSpawnPositionInCell(zone);
        Vector3 arrival = gridManager.GetArrivalPositionInCell(zone);

        if (obstaclePrefab != null)
        {
            GameObject go = Instantiate(obstaclePrefab, spawn, Quaternion.identity);
            var obstacle = go.AddComponent<ObstacleRhythmTarget>();
            obstacle.Init(spawn, arrival, zone, travelTime);
        }
    }
}
