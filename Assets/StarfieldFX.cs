using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarfieldFX : MonoBehaviour
{
    [Header("Base Settings")]
    public float baseMinVelocity = -80f;
    public float baseMaxVelocity = -140f;

    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
    }

    void Update()
    {
        if (GameSpeedManager.Instance == null) return;

        float speedMultiplier = GameSpeedManager.Instance.currentSpeed;
        var velocity = ps.velocityOverLifetime;

        // Scale the Z velocity by the current global speed
        velocity.z = new ParticleSystem.MinMaxCurve(
            baseMinVelocity * speedMultiplier,
            baseMaxVelocity * speedMultiplier
        );
    }
}
