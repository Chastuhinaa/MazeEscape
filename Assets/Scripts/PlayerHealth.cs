using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Налаштування здоров'я")]
    [Tooltip("Максимальна кількість життів (якщо немає GameManager)")]
    public int maxLives = 3;

    [Tooltip("Поточна кількість життів")]
    public int currentLives;

    [Header("Невразливість після удару")]
    public float invincibilityTime = 1.5f;
    private bool isInvincible = false;

    void Start()
    {
        ResetLives();
    }

    public void ResetLives()
    {
        int lives = (GameManager.Instance != null)
            ? GameManager.Instance.startingLives
            : maxLives;

        currentLives = lives;
    }

    public void TakeDamage()
    {
        if (isInvincible) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GameManager.GameState.Playing) return;

        currentLives--;
        Debug.Log("Отримано удар! Залишилось: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        for (int i = 0; i < 6; i++)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(invincibilityTime / 6f);
        }
        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Гравець загинув!");
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerDefeat();
    }
}
