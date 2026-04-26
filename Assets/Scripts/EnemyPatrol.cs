using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Точки патрулювання")]
    public Transform[] patrolPoints;

    [Header("Налаштування руху")]
    public float speed = 2f;

    private int currentPointIndex = 0;

    void Start()
    {
        if (patrolPoints.Length == 0)
        {
            Debug.LogError("Не призначено точок патрулювання для " + gameObject.name);
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPointIndex];

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage();
            }
        }
    }
}