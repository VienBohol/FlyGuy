using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.2f;

    [Header("Objects to Shake")]
    public Transform[] objectsToShake; // assign camera, arms, etc.

    private Vector3[] originalPositions;

    private void Awake()
    {
        if (objectsToShake != null && objectsToShake.Length > 0)
        {
            originalPositions = new Vector3[objectsToShake.Length];
            for (int i = 0; i < objectsToShake.Length; i++)
            {
                originalPositions[i] = objectsToShake[i].localPosition;
            }
        }
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            Vector3 offset = Random.insideUnitSphere * shakeMagnitude;
            offset.z = 0f; // optional, if you only want XY shake

            for (int i = 0; i < objectsToShake.Length; i++)
            {
                objectsToShake[i].localPosition = originalPositions[i] + offset;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset positions
        for (int i = 0; i < objectsToShake.Length; i++)
        {
            objectsToShake[i].localPosition = originalPositions[i];
        }
    }
}
