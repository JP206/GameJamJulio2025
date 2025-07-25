using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjust this value in the Inspector
    private Rigidbody2D rb;
    private Vector2 movementInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input from horizontal and vertical axes
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        // Normalize the vector to prevent faster diagonal movement
        movementInput.Normalize();
    }

    void FixedUpdate()
    {
        // Apply movement using Rigidbody2D velocity
        rb.linearVelocity = movementInput * moveSpeed;
    }
}
