using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool animFlag = false;

    [SerializeField] AudioSource movementAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movementAudio = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
        HandleSpriteFlip();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();
    }

    private void MovePlayer()
    {
        rb.linearVelocity = movementInput * moveSpeed;

        if (movementInput.x == 0 && movementInput.y == 0)
        {
            if (movementAudio.isPlaying)
                movementAudio.Pause();

            if (animFlag)
            {
                animator.SetTrigger("Idle");
                animFlag = false;
            }
        }
        else
        {
            if (!movementAudio.isPlaying)
                movementAudio.Play();

            if (!animFlag)
            {
                animator.SetTrigger("Run");
                animFlag = true;
            }
        }
    }

    private void HandleSpriteFlip()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        spriteRenderer.flipX = mousePos.x < transform.position.x;
    }
}
