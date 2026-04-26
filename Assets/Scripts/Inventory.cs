using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Поточний стан")]
    [Tooltip("Кількість зібраних балів")]
    public int score = 0;

    [Tooltip("Чи знайдено золотий ключ")]
    public bool hasKey = false;

    public void AddCrystal(int value)
    {
        score += value;
        Debug.Log("Зібрано кристал! Поточний рахунок: " + score);
    }

    public void SetHasKey(bool value)
    {
        hasKey = value;
        Debug.Log("Знайдено золотий ключ!");
    }
}
