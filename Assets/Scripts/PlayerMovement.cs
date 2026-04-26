using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Налаштування руху")]
    [Tooltip("Швидкість переміщення гравця")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        movementInput = movementInput.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }
}
