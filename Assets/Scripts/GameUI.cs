using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("UI елементи")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI keyStatusText;
    public TextMeshProUGUI messageText;
    public GameObject messagePanel;

    [Header("Посилання на гравця")]
    public Inventory playerInventory;
    public PlayerHealth playerHealth;

    void Update()
    {
        if (playerInventory != null)
        {
            scoreText.text = "Кристали: " + playerInventory.score;
            keyStatusText.text = playerInventory.hasKey ? " Ключ: ЗНАЙДЕНО" : " Ключ: НЕМАЄ";
            keyStatusText.color = playerInventory.hasKey ? Color.green : Color.yellow;
        }

        if (playerHealth != null)
        {
            livesText.text = "Життя: " + playerHealth.currentLives;
        }
    }

    public void ShowVictory(int finalScore)
    {
        messagePanel.SetActive(true);
        messageText.text = "ПЕРЕМОГА!\nЗібрано балів: " + finalScore;
        messageText.color = Color.green;
        Time.timeScale = 0f;
        Invoke("ReloadScene", 0f);
    }

    public void ShowDefeat()
    {
        messagePanel.SetActive(true);
        messageText.text = "ВИ ПРОГРАЛИ\nНатисніть R щоб спробувати знову";
        messageText.color = Color.red;
        Time.timeScale = 0f;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void ReloadScene()
    {
        
    }
}