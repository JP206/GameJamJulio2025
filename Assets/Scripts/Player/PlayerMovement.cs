using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField] private AudioSource movementAudio;

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
        HandleAnimation();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movementInput * moveSpeed;
    }

    private void HandleInput()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();
    }

    public void HandleAnimation()
    {
        bool isWalking = movementInput.sqrMagnitude > 0;

        animator.SetBool("isRunning", isWalking);

        if (isWalking && !movementAudio.isPlaying)
        {
            movementAudio.Play();
        }
        else if (!isWalking && movementAudio.isPlaying)
        {
            movementAudio.Pause();
        }
    }

    private void HandleSpriteFlip()
    {
        if (IntroCinematicManager.IsCinematicPlaying) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        spriteRenderer.flipX = mousePos.x < transform.position.x;
    }

    public bool GetFacingForward()
    {
        if (movementInput.x > 0)
        {
            return !spriteRenderer.flipX;
        }
        else
        {
            return spriteRenderer.flipX;
        }
    }
}
