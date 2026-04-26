using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public GameUI gameUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory inventory = other.GetComponent<Inventory>();

            if (inventory != null && inventory.hasKey)
            {
                if (gameUI != null)
                {
                    gameUI.ShowVictory(inventory.score);
                }
                Debug.Log("ПЕРЕМОГА!");
            }
            else
            {
                Debug.Log("Двері зачинені. Спочатку знайди золотий ключ!");
            }
        }
    }
}