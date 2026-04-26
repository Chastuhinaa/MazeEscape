using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum ItemType { Crystal, Key }

    [Header("Налаштування предмета")]
    [Tooltip("Тип цього предмета")]
    public ItemType itemType = ItemType.Crystal;

    [Tooltip("Скільки балів дає (для кристалів)")]
    public int scoreValue = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Inventory inventory = other.GetComponent<Inventory>();
            if (inventory != null)
            {
                if (itemType == ItemType.Crystal)
                {
                    inventory.AddCrystal(scoreValue);
                }
                else if (itemType == ItemType.Key)
                {
                    inventory.SetHasKey(true);
                }
            }

            Destroy(gameObject);
        }
    }
}
