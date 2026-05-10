using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Налаштування")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var pm = new PhysicsMaterial2D("PlayerNoFriction")
        {
            friction    = 0f,
            bounciness  = 0f
        };

        var col = GetComponent<Collider2D>();
        if (col != null) col.sharedMaterial = pm;
      
    }

    void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.State != GameManager.GameState.Playing)
        {
            input = Vector2.zero;
            return;
        }

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input   = input.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = input * moveSpeed;
    }
}
