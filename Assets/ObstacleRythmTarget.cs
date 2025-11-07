using UnityEngine;
using System.Collections;

public class ObstacleRhythmTarget : MonoBehaviour
{
    public int cellIndex;
    public float travelTime = 2f;
    public float hitWindow = 0.1f; // time around arrival that is hittable
    public bool hittable { get; private set; }

    private Vector3 startPos;
    private Vector3 endPos;
    private bool hitOrMissed = false;

    public void Init(Vector3 spawn, Vector3 arrival, int cell, float travel)
    {
        startPos = spawn;
        endPos = arrival;
        cellIndex = cell;
        travelTime = travel;
        StartCoroutine(TravelRoutine());
    }

    private IEnumerator TravelRoutine()
{
    float timer = 0f;
    float hitStart = travelTime - hitWindow; // e.g., 0.25s before arrival
    float hitEnd = travelTime + hitWindow;   // e.g., 0.25s after arrival

    while (timer < travelTime)
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / travelTime);
        transform.position = Vector3.Lerp(startPos, endPos, t);

        if (!hittable && timer >= hitStart)
            hittable = true;

        if (hittable && timer >= hitEnd)
            OnMiss(); // miss if passed hit window
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

    // Require left stick to be aiming at the obstacle cell
    if (inputCell == cellIndex && hittable && player.CurrentSelectedCell == cellIndex)
        OnHit();
}

    public void OnHit()
    {
        hitOrMissed = true;
        hittable = false;
        Destroy(gameObject);
        // later: add visual/sound feedback
    }

    private void OnMiss()
    {
        hitOrMissed = true;
        hittable = false;
        // later: reduce heart/life
        Destroy(gameObject, 0.4f);
    }
}
