using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum Difficulty { Training, Easy, Medium, Hard }
    public enum GameState  { WaitingToStart, Playing, Victory, Defeat }

    [Header("Поточна складність")]
    public Difficulty currentDifficulty = Difficulty.Training;

    [Header("Швидкість привида")]
    public float trainingGhostSpeed = 1.5f;
    public float easyGhostSpeed     = 2f;
    public float mediumGhostSpeed   = 5f;
    public float hardGhostSpeed     = 8f;

    [Header("Кількість життів")]
    public int trainingLives = 5;
    public int easyLives     = 3;
    public int mediumLives   = 3;
    public int hardLives     = 2;

    [HideInInspector] public float ghostSpeed;
    [HideInInspector] public int   startingLives;

    public GameState State       { get; private set; } = GameState.WaitingToStart;
    public float     ElapsedTime { get; private set; }

    private GameUI gameUI;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ApplyDifficulty();
    }

    void Start() => gameUI = FindAnyObjectByType<GameUI>();

    void Update()
    {
        if (State == GameState.Playing)
            ElapsedTime += Time.deltaTime;
    }

    // ── Запуск гри ────────────────────────────────────────────────────────

    public void StartGame(Difficulty difficulty)
    {
        currentDifficulty = difficulty;
        ApplyDifficulty();
        ElapsedTime = 0f;

        var mazeGen = FindAnyObjectByType<MazeGenerator>();
        if (mazeGen != null) mazeGen.Generate(difficulty);
        else Debug.LogWarning("GameManager: MazeGenerator не знайдено!");

        FindAnyObjectByType<PlayerHealth>()?.ResetLives();

        foreach (var g in FindObjectsByType<GhostAI>(FindObjectsSortMode.None))
            g.Activate();

        State = GameState.Playing;
    }

    // ── Кінець гри ────────────────────────────────────────────────────────

    public void TriggerVictory(int score)
    {
        Debug.Log($"TriggerVictory: State={State}, gameUI={gameUI}");
        if (State != GameState.Playing) return;
        State = GameState.Victory;

        // Якщо посилання втрачено — знаходимо знову
        if (gameUI == null) gameUI = FindAnyObjectByType<GameUI>();

        bool isNewRecord = false;
        float best = 0f;

        if (currentDifficulty != Difficulty.Training)
        {
            isNewRecord = TrySaveRecord(currentDifficulty, ElapsedTime);
            best = LoadRecord(currentDifficulty);
        }

        Debug.Log($"Викликаємо ShowVictory, gameUI={gameUI}");
        gameUI?.ShowVictory(score, ElapsedTime, best, isNewRecord,
                            currentDifficulty == Difficulty.Training);
    }

    public void TriggerDefeat()
    {
        if (State != GameState.Playing) return;
        State = GameState.Defeat;
        gameUI?.ShowDefeat();
    }

    // ── Рекорди ───────────────────────────────────────────────────────────

    /// <summary>Повертає рекорд для складності (0 = ще не встановлено)</summary>
    public float LoadRecord(Difficulty diff)
    {
        if (diff == Difficulty.Training) return 0f;
        return PlayerPrefs.GetFloat($"Record_{diff}", 0f);
    }

    bool TrySaveRecord(Difficulty diff, float time)
    {
        string key  = $"Record_{diff}";
        float  prev = PlayerPrefs.GetFloat(key, float.MaxValue);
        if (time < prev)
        {
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    // ── Допоміжні ─────────────────────────────────────────────────────────

    void ApplyDifficulty()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Training:
                ghostSpeed    = trainingGhostSpeed;
                startingLives = trainingLives;
                break;
            case Difficulty.Easy:
                ghostSpeed    = easyGhostSpeed;
                startingLives = easyLives;
                break;
            case Difficulty.Medium:
                ghostSpeed    = mediumGhostSpeed;
                startingLives = mediumLives;
                break;
            case Difficulty.Hard:
                ghostSpeed    = hardGhostSpeed;
                startingLives = hardLives;
                break;
        }
    }

    public string GetDifficultyName() => currentDifficulty switch
    {
        Difficulty.Training => "Тренування",
        Difficulty.Easy     => "Легко",
        Difficulty.Medium   => "Середньо",
        Difficulty.Hard     => "Важко",
        _                   => ""
    };
}
