using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDir = Vector2.right;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField] private AudioSource movementAudio;

    [Header("Roll Settings")]
    [SerializeField] private float rollForce = 10f;
    [SerializeField] private float rollCooldown = 2f;
    [SerializeField] private float rollDistance = 3f;
    [SerializeField] private float rollDuration = 0.2f;

    private float nextRollTime = 0f;
    private TrailRenderer trail;
    private bool isRolling = false;
    private bool isCastingHolyShot = false;

    public bool IsRolling => isRolling;
    public bool IsCastingHolyShot => isCastingHolyShot;

    // === Nueva bandera para knockback ===
    public bool IsKnockedBack { get; set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movementAudio = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        trail = GetComponent<TrailRenderer>();

        if (trail != null)
            trail.emitting = false;

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (UICanvasManager.IsGamePausedOrOver) return;

        HandleInput();
        HandleSpriteFlip();
        HandleAnimation();
        HandleRoll();
    }

    void FixedUpdate()
    {
        if (UICanvasManager.IsGamePausedOrOver) return;

        // Evita moverse si está siendo empujado o está casteando
        if (IsKnockedBack || isRolling || isCastingHolyShot)
            return;

        rb.linearVelocity = movementInput * moveSpeed;
    }

    private void HandleInput()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();

        if (movementInput != Vector2.zero)
        {
            lastMoveDir = movementInput;
        }
    }

    private void HandleRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextRollTime && !isCastingHolyShot)
        {
            nextRollTime = Time.time + rollCooldown;
            animator.SetTrigger("Rolling");

            Vector2 dashDir = movementInput != Vector2.zero ? movementInput : lastMoveDir;
            StartCoroutine(RollCoroutine(dashDir));
        }
    }

    private IEnumerator RollCoroutine(Vector2 dashDir)
    {
        isRolling = true;
        float elapsed = 0f;
        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + dashDir.normalized * rollDistance;

        if (trail != null) trail.emitting = true;

        while (elapsed < rollDuration)
        {
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, elapsed / rollDuration);
            rb.MovePosition(newPos);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPos);
        isRolling = false;
        if (trail != null) trail.emitting = false;
    }

    public void HandleAnimation()
    {
        bool isWalking = movementInput.sqrMagnitude > 0;
        bool facingForward = GetFacingForward();

        animator.SetBool("isRunning", facingForward && isWalking);
        animator.SetBool("isRunningBackwards", !facingForward && isWalking);

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

    public Vector2 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public void SetHolyShotState(bool state)
    {
        isCastingHolyShot = state;
        if (state)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
