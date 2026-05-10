using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI keyStatusText;
    public TextMeshProUGUI timerText;      // поточний час (рахує вгору)
    public TextMeshProUGUI recordText;     // найкращий час
    public TextMeshProUGUI difficultyText;

    [Header("Панель повідомлення")]
    public GameObject      messagePanel;
    public TextMeshProUGUI messageText;

    [Tooltip("Колір фону панелі перемоги/поразки")]
    public Color victoryBg = new(0f, 0f, 0f, 0.82f);
    public Color defeatBg  = new(0.15f, 0f, 0f, 0.88f);

    [Header("Посилання на гравця")]
    public Inventory    playerInventory;
    public PlayerHealth playerHealth;

    void Start()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    /// <summary>Якщо на панелі немає Image — додає його.</summary>
    void EnsurePanelBackground()
    {
        if (messagePanel == null) return;
        if (messagePanel.GetComponent<Image>() == null)
            messagePanel.AddComponent<Image>();
    }

    void Update()
    {
        // Кристали + ключ
        if (playerInventory != null)
        {
            if (scoreText    != null) scoreText.text = "Кристали: " + playerInventory.score;
            if (keyStatusText != null)
            {
                keyStatusText.text  = playerInventory.hasKey ? "Ключ: ЗНАЙДЕНО" : "Ключ: НЕМАЄ";
                keyStatusText.color = playerInventory.hasKey ? Color.green : Color.yellow;
            }
        }

        // Життя
        if (playerHealth != null && livesText != null)
            livesText.text = "Життя: " + playerHealth.currentLives;

        // Поточний час (рахує вгору)
        if (timerText != null && GameManager.Instance != null &&
            GameManager.Instance.State == GameManager.GameState.Playing)
        {
            timerText.text = "Час: " + FormatTime(GameManager.Instance.ElapsedTime);
            timerText.color = Color.white;
        }

        // Рекорд
        if (recordText != null && GameManager.Instance != null &&
            GameManager.Instance.State == GameManager.GameState.Playing)
        {
            float rec = GameManager.Instance.LoadRecord(GameManager.Instance.currentDifficulty);
            recordText.text = rec > 0f ? "Рекорд: " + FormatTime(rec) : "Рекорд: —";
        }

        // Складність
        if (difficultyText != null && GameManager.Instance != null &&
            GameManager.Instance.State == GameManager.GameState.Playing)
            difficultyText.text = GameManager.Instance.GetDifficultyName();

        // Перезапуск по R
        if (Input.GetKeyDown(KeyCode.R) &&
            GameManager.Instance != null &&
            GameManager.Instance.State != GameManager.GameState.Playing &&
            GameManager.Instance.State != GameManager.GameState.WaitingToStart)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // ── Кінцеві екрани ────────────────────────────────────────────────────

    public void ShowVictory(int score, float elapsed, float best, bool isNewRecord,
                            bool isTraining = false)
    {
        Debug.Log($"[GameUI] ShowVictory: messagePanel={messagePanel}, messageText={messageText}");

        if (messagePanel == null)
        {
            Debug.LogError("[GameUI] messagePanel == null! Перевір в Inspector що поле MessagePanel заповнене.");
        }
        else
        {
            messagePanel.SetActive(true);
            Debug.Log($"[GameUI] messagePanel.SetActive(true). activeSelf={messagePanel.activeSelf}, activeInHierarchy={messagePanel.activeInHierarchy}");
            SetPanelBg(victoryBg);
        }

        string recLine;
        if (isTraining)
            recLine = $"Час тренування: {FormatTime(elapsed)}";
        else if (isNewRecord)
            recLine = $"<color=yellow>НОВИЙ РЕКОРД! {FormatTime(elapsed)}</color>";
        else
            recLine = $"Ваш час: {FormatTime(elapsed)}\nРекорд:  {(best > 0f ? FormatTime(best) : "—")}";

        if (messageText == null)
        {
            Debug.LogError("[GameUI] messageText == null! Перевір в Inspector що поле MessageText заповнене.");
        }
        else
        {
            messageText.text  = $"ПЕРЕМОГА!\nКристалів: {score}\n{recLine}\n\nR — спробувати знову";
            messageText.color = Color.green;
            Debug.Log($"[GameUI] messageText.text встановлено. enabled={messageText.enabled}");
        }

        Time.timeScale = 0f;
        Debug.Log("[GameUI] Time.timeScale = 0f");
    }

    public void ShowDefeat()
    {
        Debug.Log($"[GameUI] ShowDefeat: messagePanel={messagePanel}, messageText={messageText}");

        if (messagePanel == null)
        {
            Debug.LogError("[GameUI] messagePanel == null!");
        }
        else
        {
            messagePanel.SetActive(true);
            Debug.Log($"[GameUI] messagePanel активний: activeSelf={messagePanel.activeSelf}, activeInHierarchy={messagePanel.activeInHierarchy}");
            SetPanelBg(defeatBg);
        }

        if (messageText != null)
        {
            messageText.text  = "ВИ ПРОГРАЛИ\nR — спробувати знову";
            messageText.color = Color.red;
        }

        Time.timeScale = 0f;
    }

    void SetPanelBg(Color color)
    {
        if (messagePanel == null) return;
        EnsurePanelBackground();
        var img = messagePanel.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    // ── Утиліти ───────────────────────────────────────────────────────────

    static string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}
