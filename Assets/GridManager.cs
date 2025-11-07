using UnityEngine;

/// <summary>
/// Calculates a camera-relative 3x3 grid of arrival and spawn points.
/// Arrival points are at arrivalDistance from the camera; spawn points are at spawnDistance.
/// Uses Viewport coordinates to ensure alignment across FOV/resolution.
/// This version computes positions and then locks them at Start (so camera movement won't re-bake them).
/// </summary>
[ExecuteAlways]
public class GridManager : MonoBehaviour
{
    public Camera targetCamera;            // assign Main Camera (or left null to auto-find)
    [Range(1, 20)] public int rows = 3;    // vertical count (keep 3)
    [Range(1, 20)] public int cols = 3;    // horizontal count (keep 3)

    [Tooltip("Z distance from camera for arrival (units forward along camera forward)")]
    public float arrivalDistance = 2f;     // close to camera (impact point)
    [Tooltip("Z distance from camera for spawn (farther back)")]
    public float spawnDistance = 50f;      // spawn far out so player doesn't see pop in

    [Tooltip("Number of points to jitter inside each cell in viewport space (0..0.5)")]
    public float viewportJitter = 0.2f;    // small random offset inside cell in viewport space

    [Header("Runtime / Debug")]
    [Tooltip("If true, grid will be computed at Start and then locked during Play. Turn off to keep dynamic.")]
    public bool lockGridAfterStart = true;

    // computed positions (world-space)
    public Vector3[] arrivalWorldPositions;
    public Vector3[] spawnWorldPositions;

    private bool gridLocked = false;

    private void OnValidate()
    {
        if (rows < 1) rows = 1;
        if (cols < 1) cols = 1;

        // allow preview in editor
        if (!Application.isPlaying)
            RecomputeGrid();
    }

    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        RecomputeGrid();
    }

    void Start()
    {
        // lock grid at start of play so moving camera won't change world positions
        if (Application.isPlaying && lockGridAfterStart)
            gridLocked = true;
    }

    void Update()
    {
#if UNITY_EDITOR
        // In the editor (not playing), we want to show updated gizmos if camera moved.
        // But if playing and locked, do nothing.
        if (!Application.isPlaying)
        {
            if (targetCamera == null) targetCamera = Camera.main;
            RecomputeGrid();
        }
#endif
    }

    /// <summary>
    /// Recompute the grid positions (world space) for arrival and spawn planes using viewport coordinates.
    /// Indexing: row-major (r then c). Example: idx = r * cols + c
    /// This method will early-out if gridLocked is true (so positions stay static at runtime after Start).
    /// </summary>
    public void RecomputeGrid()
    {
        if (gridLocked) return;
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null) return;

        int total = rows * cols;
        if (arrivalWorldPositions == null || arrivalWorldPositions.Length != total)
            arrivalWorldPositions = new Vector3[total];
        if (spawnWorldPositions == null || spawnWorldPositions.Length != total)
            spawnWorldPositions = new Vector3[total];

        // cell size in viewport (0..1)
        float cellW = 1f / cols;
        float cellH = 1f / rows;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;

                // center of cell in viewport space
                float vx = (c + 0.5f) * cellW;
                float vy = 1f - ((r + 0.5f) * cellH); // invert Y because viewport Y=0 bottom in world

                // arrival point center
                Vector3 arrivalViewport = new Vector3(vx, vy, arrivalDistance);
                arrivalWorldPositions[idx] = targetCamera.ViewportToWorldPoint(arrivalViewport);

                // spawn point center (same viewport pos but different z)
                Vector3 spawnViewport = new Vector3(vx, vy, spawnDistance);
                spawnWorldPositions[idx] = targetCamera.ViewportToWorldPoint(spawnViewport);
            }
        }
    }

    /// <summary>
    /// Returns a spawn position within the cell's viewport rectangle with optional randomness.
    /// zoneIndex expected 0..rows*cols-1
    /// </summary>
    public Vector3 GetSpawnPositionInCell(int zoneIndex)
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (arrivalWorldPositions == null)
            RecomputeGrid();

        zoneIndex = Mathf.Clamp(zoneIndex, 0, rows * cols - 1);

        int r = zoneIndex / cols;
        int c = zoneIndex % cols;

        float cellW = 1f / cols;
        float cellH = 1f / rows;

        // base center in viewport coords
        float vx = (c + 0.5f) * cellW;
        float vy = 1f - ((r + 0.5f) * cellH);

        // apply viewport jitter in [-viewportJitter..viewportJitter] * half cell size
        float jitterX = (Random.value * 2f - 1f) * viewportJitter * cellW * 0.5f;
        float jitterY = (Random.value * 2f - 1f) * viewportJitter * cellH * 0.5f;

        Vector3 spawnViewport = new Vector3(vx + jitterX, vy + jitterY, spawnDistance);
        return targetCamera.ViewportToWorldPoint(spawnViewport);
    }

    /// <summary>
    /// Returns the arrival (impact) world position for the cell with optional small jitter (used for variety).
    /// </summary>
    public Vector3 GetArrivalPositionInCell(int zoneIndex)
    {
        if (arrivalWorldPositions == null)
            RecomputeGrid();

        zoneIndex = Mathf.Clamp(zoneIndex, 0, rows * cols - 1);
        return arrivalWorldPositions[zoneIndex];
    }

    /// <summary>
    /// Returns a rows x cols 2D array of arrival positions (row, col).
    /// </summary>
    public Vector3[,] GetArrivalPoints2D()
    {
        if (arrivalWorldPositions == null)
            RecomputeGrid();

        Vector3[,] result = new Vector3[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                result[r, c] = arrivalWorldPositions[r * cols + c];
        return result;
    }

    private void OnDrawGizmos()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        // Show gizmos using current baked positions (do not force recompute at runtime if locked).
        if (arrivalWorldPositions == null || spawnWorldPositions == null)
            RecomputeGrid();

        Gizmos.color = Color.cyan;
        if (arrivalWorldPositions != null)
        {
            for (int i = 0; i < arrivalWorldPositions.Length; i++)
            {
                Gizmos.DrawWireSphere(arrivalWorldPositions[i], 0.3f);
                if (targetCamera != null)
                    Gizmos.DrawLine(arrivalWorldPositions[i], arrivalWorldPositions[i] + targetCamera.transform.forward * -0.5f);
            }
        }

        Gizmos.color = Color.yellow;
        if (spawnWorldPositions != null)
        {
            for (int i = 0; i < spawnWorldPositions.Length; i++)
            {
                Gizmos.DrawWireSphere(spawnWorldPositions[i], 0.25f);
                if (arrivalWorldPositions != null && i < arrivalWorldPositions.Length)
                    Gizmos.DrawLine(spawnWorldPositions[i], arrivalWorldPositions[i]);
            }
        }
    }
}
