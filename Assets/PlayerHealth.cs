using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxLives = 3;
    private int currentLives;

    [Header("UI")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private void OnEnable()
    {
        ObstacleRhythmTarget.OnPlayerMiss += TakeDamage;
    }

    private void OnDisable()
    {
        ObstacleRhythmTarget.OnPlayerMiss -= TakeDamage;
    }

    private void Start()
    {
        currentLives = maxLives;
        UpdateHearts();
    }

    private void TakeDamage()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        UpdateHearts();

        if (currentLives <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < currentLives ? fullHeart : emptyHeart;
        }
    }
}
