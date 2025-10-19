using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float speed, invulnerabilityTime = 2f, flashInterval = 0.1f;
    [SerializeField] int damage, maxHp, currentHp;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hit1, hit2, hit3;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] bool bossChicken;

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false, isDead = false, celebrating = false;
    private Animator animator;
    private WaveManager waveManager;
    private Collider2D collider;
    private Rigidbody2D rb;

    private Vector2 avoidanceDirection = Vector2.zero;

    // === Ajustes del Raycast ===
    [Header("Detection Settings")]
    [SerializeField] private float detectionDistance = 5f;
    [SerializeField] private float rayAngle = 30f;
    [SerializeField] private LayerMask obstacleMask;

    // === Estado de evasión ===
    private bool isAvoiding = false;
    private float avoidTimer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerTransform = player.transform;
        }

        if (hitEffect == null)
        {
            hitEffect = GetComponentInChildren<ParticleSystem>();
        }

        SetBossAsTrue();
    }

    private void OnEnable()
    {
        isDead = false;
        currentHp = maxHp;

        if (hitEffect != null)
        {
            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void SetWaveManager(WaveManager _waveManager)
    {
        waveManager = _waveManager;
    }

    void FixedUpdate()
    {
        if (playerTransform && !isDead && !celebrating && rb != null)
        {
            Vector2 direction;

            // === MODO EVASIÓN ===
            if (isAvoiding)
            {
                // Combina la evasión con un porcentaje de dirección hacia el jugador
                Vector2 toPlayer = (playerTransform.position - transform.position).normalized;
                direction = (avoidanceDirection * 0.7f + toPlayer * 0.3f).normalized;

                avoidTimer -= Time.fixedDeltaTime;
                if (avoidTimer <= 0f)
                {
                    isAvoiding = false;
                    avoidanceDirection = Vector2.zero;
                }
            }
            else
            {
                direction = (playerTransform.position - transform.position).normalized;
            }

            // Detectar obstáculos
            DetectObstacles(ref direction);

            direction = direction.normalized;
            Vector2 moveDelta = direction * speed * Time.fixedDeltaTime;

            // --- Prevención de atravesar obstáculos ---
            RaycastHit2D hit = Physics2D.Raycast(rb.position, moveDelta.normalized, moveDelta.magnitude + 0.1f, obstacleMask);

            if (hit.collider == null)
            {
                // No hay colisión → moverse normal
                rb.MovePosition(rb.position + moveDelta);
            }
            else
            {
                // Si hay colisión → deslizarse sobre el obstáculo
                Vector2 slideDir = Vector2.Perpendicular(hit.normal).normalized;
                Vector2 slideMove = slideDir * speed * 0.5f * Time.fixedDeltaTime;
                rb.MovePosition(rb.position + slideMove);
            }

            float movementMagnitude = moveDelta.magnitude;

            if (animator != null && HasParameter(animator, "isMoving"))
                animator.SetBool("isMoving", movementMagnitude > 0.01f);

            // 🧠 Si el enemigo casi no se movió, salir de evasión
            if (movementMagnitude < 0.01f)
            {
                isAvoiding = false;
                avoidanceDirection = Vector2.zero;
            }

            // Flip visual según posición del jugador
            float xDiff = transform.position.x - playerTransform.position.x;
            spriteRenderer.flipX = xDiff < 0;
        }
        else
        {
            if (animator != null && HasParameter(animator, "isMoving"))
                animator.SetBool("isMoving", false);
        }
    }

    private void DetectObstacles(ref Vector2 direction)
    {
        Vector2 origin = transform.position;

        Vector2 forwardDir = direction.normalized;
        Vector2 leftDir = Quaternion.Euler(0, 0, rayAngle) * forwardDir;
        Vector2 rightDir = Quaternion.Euler(0, 0, -rayAngle) * forwardDir;

        RaycastHit2D hitFront = Physics2D.Raycast(origin, forwardDir, detectionDistance, obstacleMask);
        RaycastHit2D hitLeft = Physics2D.Raycast(origin, leftDir, detectionDistance, obstacleMask);
        RaycastHit2D hitRight = Physics2D.Raycast(origin, rightDir, detectionDistance, obstacleMask);

        // Debug visual
        Debug.DrawRay(origin, forwardDir * detectionDistance, hitFront.collider ? Color.red : Color.green);
        Debug.DrawRay(origin, leftDir * detectionDistance, hitLeft.collider ? Color.yellow : Color.green);
        Debug.DrawRay(origin, rightDir * detectionDistance, hitRight.collider ? Color.cyan : Color.green);

        // === LÓGICA DE EVASIÓN ===
        if (hitFront.collider != null && (hitFront.collider.CompareTag("Building") || hitFront.collider.CompareTag("Fire")))
        {
            bool leftFree = hitLeft.collider == null;
            bool rightFree = hitRight.collider == null;

            Vector2 avoidDir = Vector2.zero;

            if (leftFree && !rightFree)
                avoidDir = leftDir;
            else if (rightFree && !leftFree)
                avoidDir = rightDir;
            else if (leftFree && rightFree)
                avoidDir = rightDir; // preferir derecha
            else
                avoidDir = Quaternion.Euler(0, 0, -rayAngle * 1.5f) * forwardDir; // giro fuerte

            // 💡 Solo iniciar evasión si no estaba ya evitando
            if (!isAvoiding)
            {
                avoidanceDirection = avoidDir.normalized;
                isAvoiding = true;
                avoidTimer = 1.2f; // más tiempo para rodear
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DoDamage(collision);

        if (collision.CompareTag("Player"))
        {
            if (!isDead)
            {
                PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
                PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

                if (playerMovement != null && !playerMovement.IsRolling && playerHealth != null)
                {
                    if (animator != null && HasParameter(animator, "Attack"))
                        animator.SetTrigger("Attack");

                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    private void DoDamage(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
            TakeDamage(1);
        else if (collision.CompareTag("HolyBullet"))
            TakeDamage(5);
    }

    private void TakeDamage(int amount)
    {
        if (!isInvulnerable && !isDead)
        {
            audioSource.PlayOneShot(GethitSound());
            currentHp -= amount;

            if (hitEffect != null)
            {
                hitEffect.gameObject.SetActive(true);
                hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                hitEffect.Play();
            }

            if (currentHp <= 0)
                StartCoroutine(Death());
            else
                StartCoroutine(DamageFlashAndInvulnerability());
        }
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        isInvulnerable = true;
        spriteRenderer.color = Color.red;
        collider.enabled = false;

        if (bossChicken && animator != null && HasParameter(animator, "GetHit"))
            animator.SetTrigger("GetHit");

        yield return new WaitForSeconds(0.1f);

        float elapsed = 0f;
        while (elapsed < invulnerabilityTime)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashInterval / 2f);
            spriteRenderer.color = Color.clear;
            yield return new WaitForSeconds(flashInterval / 2f);
            elapsed += flashInterval;
        }

        spriteRenderer.color = Color.white;
        isInvulnerable = false;
        collider.enabled = true;
    }

    private IEnumerator Death()
    {
        isDead = true;

        if (waveManager != null)
            waveManager.NotifyDeath();

        if (animator != null && HasParameter(animator, "Death"))
            animator.SetTrigger("Death");

        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(false);

        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(true);
            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitEffect.Play();
        }

        yield return null;
        yield return new WaitForSeconds(animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0f);

        if (hitEffect != null)
            yield return new WaitForSeconds(hitEffect.main.duration);

        gameObject.SetActive(false);
    }

    private AudioClip GethitSound()
    {
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0: return hit1;
            case 1: return hit2;
            case 2: return hit3;
        }
        return null;
    }

    public void Celebrate()
    {
        celebrating = true;
        if (animator != null && HasParameter(animator, "Celebration"))
            animator.SetTrigger("Celebration");
    }

    private void SetBossAsTrue()
    {
        if (gameObject.CompareTag("Boss"))
            bossChicken = true;
    }

    private bool HasParameter(Animator anim, string paramName)
    {
        foreach (var param in anim.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}
