using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 3;
    public float regenDelay = 10f;
    public float regenRate = 1f;

    [Header("References")]
    public UnityEngine.UI.Image[] heartImages; // Updated for UI Image
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public CameraShake cameraShake;           // assign in inspector
    public AudioSource audioSource;           // assign in inspector
    public AudioClip damageClip;              // assign a sound effect here

    private int currentHealth;
    private float lastDamageTime;
    private Coroutine regenRoutine;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHearts();
        lastDamageTime = Time.time;
    }

    private void OnEnable()
    {
        ObstacleRhythmTarget.OnPlayerMiss += TakeDamage;
    }

    private void OnDisable()
    {
        ObstacleRhythmTarget.OnPlayerMiss -= TakeDamage;
    }

    public void TakeDamage()
    {
        if (currentHealth <= 0) return;

        currentHealth--;
        lastDamageTime = Time.time;
        UpdateHearts();

        // Trigger screen shake and sound
        if (cameraShake != null)
            cameraShake.Shake();

        if (audioSource != null && damageClip != null)
            audioSource.PlayOneShot(damageClip);

        if (regenRoutine != null)
        {
            StopCoroutine(regenRoutine);
            regenRoutine = null;
        }

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        regenRoutine = StartCoroutine(RegenRoutine());
    }

    private IEnumerator RegenRoutine()
    {
        while (Time.time - lastDamageTime < regenDelay)
            yield return null;

        while (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHearts();

            if (Time.time - lastDamageTime < regenDelay)
                yield break;

            yield return new WaitForSeconds(regenRate);
        }

        regenRoutine = null;
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }
}
