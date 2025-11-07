using UnityEngine;
using System.Collections;

public class ObstacleRhythmTarget : MonoBehaviour
{
    public int cellIndex;
    public float travelTime = 2f;
    public float hitWindow = 0.1f;
    public bool hittable { get; private set; }

    private Vector3 startPos;
    private Vector3 endPos;
    private bool hitOrMissed = false;
     public float indicatorHeight = 1.0f;

    private GameObject[] indicators;

    /// <summary>
    /// Initializes the obstacle and spawns its indicators above it.
    /// </summary>
    public void Init(Vector3 spawn, Vector3 arrival, int cell, float travel, GameObject indicatorPrefab, Sprite[] sprites)
    {
        startPos = spawn;
        endPos = arrival;
        cellIndex = cell;
        travelTime = travel;

        // Spawn indicators
        if (indicatorPrefab != null && sprites != null && sprites.Length > 0)
        {
            indicators = new GameObject[sprites.Length];
            float offsetStep = 0.7f; // spacing for multiple indicators
            float startOffset = -(sprites.Length - 1) * offsetStep / 2f;

            for (int i = 0; i < sprites.Length; i++)
            {
                GameObject ind = Instantiate(indicatorPrefab, transform);
                ind.transform.localPosition = Vector3.up * indicatorHeight + Vector3.right * (startOffset + i * offsetStep);
                var sr = ind.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = sprites[i];
                indicators[i] = ind;
            }
        }

        StartCoroutine(TravelRoutine());
    }

    private IEnumerator TravelRoutine()
    {
        float timer = 0f;
        float hitStart = travelTime - hitWindow;
        float hitEnd = travelTime + hitWindow;

        while (timer < travelTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / travelTime);
            transform.position = Vector3.Lerp(startPos, endPos, t);

            if (!hittable && timer >= hitStart)
                hittable = true;

            if (hittable && timer >= hitEnd)
                OnMiss();

            yield return null;
        }

        if (!hitOrMissed)
            OnMiss();
    }

    private void OnEnable() => PlayerRhythmInput.OnCellInput += TryHandleHit;
    private void OnDisable() => PlayerRhythmInput.OnCellInput -= TryHandleHit;

    private void TryHandleHit(int inputCell)
    {
        var player = FindFirstObjectByType<PlayerRhythmInput>();
        if (player == null) return;

        // Require both correct input and aiming at the correct cell
        if (inputCell == cellIndex && hittable && player.CurrentSelectedCell == cellIndex)
            OnHit();
    }

    public void OnHit()
    {
        hitOrMissed = true;
        hittable = false;
        Destroy(gameObject);
        // optional: play visual/sound feedback
    }

    private void OnMiss()
    {
        hitOrMissed = true;
        hittable = false;
        Destroy(gameObject, 0.4f);
        // optional: reduce player life
    }
}
