using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public int maxHealth = 3;
    public float regenDelay = 10f; // seconds without damage before regen starts
    public float regenRate = 1f;   // seconds between each heart regen

    [Header("References")]
    public SpriteRenderer[] heartSprites;
    public Sprite fullHeart;
    public Sprite emptyHeart;

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
        // Wait for regenDelay seconds without taking damage
        while (Time.time - lastDamageTime < regenDelay)
            yield return null;

        // Start regenerating 1 heart per regenRate seconds
        while (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHearts();

            // Stop if damaged again
            if (Time.time - lastDamageTime < regenDelay)
                yield break;

            yield return new WaitForSeconds(regenRate);
        }

        regenRoutine = null;
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < heartSprites.Length; i++)
        {
            heartSprites[i].sprite = i < currentHealth ? fullHeart : emptyHeart;
        }
    }
}
