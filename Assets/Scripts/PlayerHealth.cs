using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Налаштування здоров'я")]
    [Tooltip("Початкова кількість життів")]
    public int maxLives = 3;

    [Tooltip("Поточна кількість життів")]
    public int currentLives;

    [Header("Посилання")]
    public GameUI gameUI;

    [Header("Невразливість після удару")]
    public float invincibilityTime = 1.5f;
    private bool isInvincible = false;

    void Start()
    {
        currentLives = maxLives;
    }

    public void TakeDamage()
    {
        if (isInvincible) return;

        currentLives--;
        Debug.Log("Втрачено життя! Залишилось: " + currentLives);

        if (currentLives <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrames());
        }
    }

    private System.Collections.IEnumerator InvincibilityFrames()
    {
        isInvincible = true;
        // Мерехтіння спрайта
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        for (int i = 0; i < 6; i++)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(invincibilityTime / 6);
        }
        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Гравець загинув!");
        if (gameUI != null)
        {
            gameUI.ShowDefeat();
        }
    }
}