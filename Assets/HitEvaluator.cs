using UnityEngine;

[System.Serializable]
public class HitEvaluator
{
    [Header("Hit Window Settings")]
    public float earlyWindow = 0.8f; // seconds before arrival
    public float lateWindow = 0.2f;  // seconds after arrival

    // Total window = 0.5 seconds

    public string EvaluateHit(float timeUntilArrival)
    {
        if (timeUntilArrival < -earlyWindow || timeUntilArrival > lateWindow)
            return "Miss";

        float absTime = Mathf.Abs(timeUntilArrival);

        if (absTime < 0.05f) return "Perfect";
        if (absTime < 0.1f) return "Great";
        if (absTime < 0.2f) return "Good";
        return "Bad";
    }

    public bool IsWithinWindow(float timeUntilArrival)
    {
        return !(timeUntilArrival < -earlyWindow || timeUntilArrival > lateWindow);
    }
}
