using UnityEngine;

[ExecuteAlways]
public class SimpleSkyboxRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 1f;      // overall speed multiplier
    public float maxTilt = 1f;            // max degrees for pitch and roll
    public float noiseScale = 0.1f;       // how fast the noise changes

    private void Update()
    {
        float time = Time.time * noiseScale;

        // Use Perlin noise to generate smooth, small rotations
        float yaw = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * rotationSpeed;
        float pitch = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * maxTilt;
        float roll = (Mathf.PerlinNoise(time, time) - 0.5f) * 2f * maxTilt;

        // Apply the yaw to the skybox rotation
        RenderSettings.skybox.SetFloat("_Rotation", RenderSettings.skybox.GetFloat("_Rotation") + yaw * Time.deltaTime);

        // Optionally, if your shader supports a vector for tilt, you could pass pitch/roll:
        //
         RenderSettings.skybox.SetVector("_RotationAngles", new Vector3(pitch, 0f, roll));
    }
}
