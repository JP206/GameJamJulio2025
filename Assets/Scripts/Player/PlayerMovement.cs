using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDir = Vector2.right; // dirección por defecto (derecha)

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [SerializeField] private AudioSource movementAudio;

    [Header("Roll Settings")]
    [SerializeField] private float rollForce = 10f;
    [SerializeField] private float rollCooldown = 2f;

    private float nextRollTime = 0f;
    private TrailRenderer trail;
    private bool isRolling = false;

    public bool IsRolling => isRolling;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        movementAudio = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        trail = GetComponent<TrailRenderer>();

        if (trail != null)
            trail.emitting = false; // arrancamos apagado
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

        if (!isRolling) // solo moverse si no está rodando
        {
            rb.linearVelocity = movementInput * moveSpeed;
        }
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
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextRollTime)
        {
            nextRollTime = Time.time + rollCooldown;

            // Trigger animación
            animator.SetTrigger("Rolling");

            // Dirección de dash: actual o última conocida
            Vector2 dashDir = movementInput != Vector2.zero ? movementInput : lastMoveDir;

            // Resetear velocidad y aplicar impulso
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(dashDir * rollForce, ForceMode2D.Impulse);

            // Bloquear movimiento normal durante el roll
            StartCoroutine(RollCoroutine());

            // Activar Trail
            if (trail != null)
                StartCoroutine(RollTrail());
        }
    }

    private IEnumerator RollCoroutine()
    {
        isRolling = true;
        yield return new WaitForSeconds(0.5f); // duración del dash
        isRolling = false;
    }

    private IEnumerator RollTrail()
    {
        trail.emitting = true;
        yield return new WaitForSeconds(0.5f); // mismo tiempo que el dash
        trail.emitting = false;
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
}
