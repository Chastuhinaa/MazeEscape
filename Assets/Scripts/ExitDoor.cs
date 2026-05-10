using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory == null) return;

        if (inventory.hasKey)
        {
            Debug.Log("Вихід відкрито! Перемога!");
            if (GameManager.Instance != null)
                GameManager.Instance.TriggerVictory(inventory.score);
        }
        else
        {
            Debug.Log("Двері зачинено. Спочатку знайдіть ключ!");
        }
    }
}
