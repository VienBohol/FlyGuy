using UnityEngine;

public class ObstacleIndicator : MonoBehaviour
{
    [Header("Scale Settings")]
    public float minScale = 0.5f;
    public float maxScale = 1f;
    public float scaleDistance = 10f; // distance at which scale is max

    private Transform cam;

    void Awake()
    {
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        // Always face the camera
        transform.forward = cam.forward;

        // Optional: scale based on distance to camera
        float dist = Vector3.Distance(transform.position, cam.position);
        float t = Mathf.Clamp01(dist / scaleDistance);
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
