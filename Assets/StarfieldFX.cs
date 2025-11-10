using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarfieldFX : MonoBehaviour
{
    [Header("Base Settings")]
    public float baseMinVelocity = -80f;
    public float baseMaxVelocity = -140f;

    [Header("Z Stretch Settings")]
    public float baseZStretch = 1f;
    public float maxZStretch = 4f;

    [Header("Emission Settings")]
    public float baseEmission = 60f;
    public float maxEmission = 220f;

    private ParticleSystem ps;
    private ParticleSystem.VelocityOverLifetimeModule velocity;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emission;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        mainModule = ps.main;
        emission = ps.emission;
    }

    void Update()
    {
        if (GameSpeedManager.Instance == null) return;

        float speed = GameSpeedManager.Instance.currentSpeed;
        float nonlinear = speed * speed; // dramatic increase

        // Velocity scaling
        float minV = Mathf.Clamp(baseMinVelocity * nonlinear, baseMinVelocity, baseMinVelocity * 5f);
        float maxV = Mathf.Clamp(baseMaxVelocity * nonlinear, baseMaxVelocity, baseMaxVelocity * 5f);
        velocity.z = new ParticleSystem.MinMaxCurve(minV, maxV);

        // Z Stretch
        mainModule.startSizeZ = Mathf.Clamp(baseZStretch * nonlinear, baseZStretch, maxZStretch);

        // Emission scaling
        emission.rateOverTime = Mathf.Clamp(baseEmission * nonlinear, baseEmission, maxEmission);
    }
}
