using UnityEngine;

public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance { get; private set; }

    [Header("Speed Settings")]
    public float baseSpeed = 1f;      // starting speed
    public float maxSpeed = 3f;       // top speed
    public float rampTime = 120f;     // seconds to reach max

    [Header("Debug Info")]
[SerializeField] private float debugCurrentSpeed; // visible in Inspector

    public float currentSpeed { get; private set; } = 1f;

    private float elapsedTime = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / rampTime);
        currentSpeed = Mathf.Lerp(baseSpeed, maxSpeed, t * t);

        debugCurrentSpeed = currentSpeed; // update Inspector display
    }
}
