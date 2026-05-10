using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Панель вибору режиму. Знаходить кнопки АВТОМАТИЧНО по назві:
///   TrainingButton, EasyButton, MediumButton, HardButton, DescriptionText
/// </summary>
public class DifficultySelector : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 0f;

        Bind("TrainingButton", GameManager.Difficulty.Training);
        Bind("EasyButton",     GameManager.Difficulty.Easy);
        Bind("MediumButton",   GameManager.Difficulty.Medium);
        Bind("HardButton",     GameManager.Difficulty.Hard);

        var desc = FindTMP("DescriptionText");
        if (desc != null) desc.text = "Оберіть режим\n(або 0 — тренування, 1/2/3 — рівні)";

        AddHover("TrainingButton",
            "ТРЕНУВАННЯ\nТвій оригінальний лабіринт\nПривид: дуже повільний\nЖиття: 5\nРекорд не зберігається", desc);
        AddHover("EasyButton",
            "ЛЕГКО\nМаленький лабіринт\nПривид: повільний\nЖиття: 3", desc);
        AddHover("MediumButton",
            "СЕРЕДНЬО\nСередній лабіринт\nПривид: швидкий\nЖиття: 3", desc);
        AddHover("HardButton",
            "ВАЖКО\nВеликий лабіринт\n2 привиди: дуже швидкі\nЖиття: 2", desc);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
            Select(GameManager.Difficulty.Training);
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            Select(GameManager.Difficulty.Easy);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            Select(GameManager.Difficulty.Medium);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            Select(GameManager.Difficulty.Hard);
    }

    void Bind(string childName, GameManager.Difficulty diff)
    {
        var btn = FindButton(childName);
        btn?.onClick.AddListener(() => Select(diff));
    }

    void Select(GameManager.Difficulty diff)
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        GameManager.Instance?.StartGame(diff);
    }

    // ── Підказки при наведенні ────────────────────────────────────────────

    void AddHover(string childName, string text, TextMeshProUGUI label)
    {
        if (label == null) return;
        var btn = FindButton(childName);
        if (btn == null) return;

        var trigger = btn.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>()
                   ?? btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var enter = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => label.text = text);
        trigger.triggers.Add(enter);

        var exit = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => label.text = "Оберіть режим\n(або 0 — тренування, 1/2/3 — рівні)");
        trigger.triggers.Add(exit);
    }

    // ── Пошук дочірніх елементів ─────────────────────────────────────────

    Button FindButton(string childName)
    {
        var t = transform.Find(childName);
        if (t == null) { Debug.LogWarning($"DifficultySelector: не знайдено '{childName}'"); return null; }
        return t.GetComponent<Button>();
    }

    TextMeshProUGUI FindTMP(string childName)
    {
        var t = transform.Find(childName);
        return t?.GetComponent<TextMeshProUGUI>();
    }
}
