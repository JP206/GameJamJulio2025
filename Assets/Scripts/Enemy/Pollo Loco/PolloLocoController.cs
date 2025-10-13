using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PolloLocoController : MonoBehaviour
{
    [SerializeField] float speed, invulnerabilityTime, flashInterval;
    [SerializeField] int damage, maxHp, currentHp;
    [SerializeField] float attackRange;
    [SerializeField] float attackCooldown = 1.8f;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip hit1, hit2, hit3;
    [SerializeField] private ParticleSystem hitEffect;

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false, isDead = false, celebrating = false, isAttacking = false;
    private Animator animator;
    private WaveManager waveManager;
    private Collider2D myCollider;
    private Rigidbody2D rb;
    private Vector2 avoidanceDirection = Vector2.zero;
    private float lastAttackTime;

    [Header("Area Attack Settings")]
    [SerializeField] private float areaJumpDuration = 1f;
    [SerializeField] private float areaJumpMaxDistance = 8f;
    [SerializeField] private float areaJumpHeight = 1.5f;
    [SerializeField] private float areaImpactRadius = 1.5f;
    [SerializeField, Tooltip("Distancia mínima para que el panzazo se ejecute")]
    private float minAreaAttackDistance = 3f;
    [SerializeField, Tooltip("Qué tan corto cae respecto al jugador (0 = exacto encima, 0.2 = 20% antes)")]
    private float landingOffsetFactor = 0.15f;

    private bool isAreaJumping = false;
    private Vector2 jumpStartPos, jumpEndPos;
    private float jumpElapsed;

    [Header("UI")]
    [SerializeField] private Slider bossHealthSlider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
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
    }

    private void OnEnable()
    {
        isDead = false;
        currentHp = maxHp;
        lastAttackTime = -attackCooldown;
        isAttacking = false;
        isAreaJumping = false;

        if (hitEffect != null)
        {
            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHp;
            bossHealthSlider.value = currentHp;
        }
    }

    public void SetWaveManager(WaveManager _waveManager)
    {
        waveManager = _waveManager;
    }

    void FixedUpdate()
    {
        if (isAreaJumping)
        {
            jumpElapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(jumpElapsed / areaJumpDuration);

            if (playerTransform != null)
            {
                Vector2 dir = ((Vector2)playerTransform.position - jumpStartPos).normalized;
                float dist = Mathf.Min(Vector2.Distance(jumpStartPos, playerTransform.position), areaJumpMaxDistance);
                dist *= (1f - landingOffsetFactor);
                jumpEndPos = jumpStartPos + dir * dist;
            }

            float accel = t * t;
            Vector2 pos = Vector2.Lerp(jumpStartPos, jumpEndPos, accel);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * areaJumpHeight;
            rb.MovePosition(pos + Vector2.up * heightOffset);

            if (t >= 1f)
            {
                rb.MovePosition(jumpEndPos);
                isAreaJumping = false;
                isAttacking = false;
                jumpElapsed = 0f;
                ExecuteAreaImpact();
            }
            return;
        }

        if (playerTransform && !isDead && !celebrating && rb != null)
        {
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            if (avoidanceDirection != Vector2.zero)
                direction = Vector2.Lerp(direction, avoidanceDirection, 0.5f);

            direction = direction.normalized;
            Vector2 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
            animator.SetBool("isMoving", true);
            rb.MovePosition(newPosition);

            spriteRenderer.flipX = (transform.position.x - playerTransform.position.x) < 0;

            float distanceToPlayer = Vector2.Distance(playerTransform.position, transform.position);

            if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                if (distanceToPlayer <= attackRange)
                {
                    animator.SetTrigger("Attack");
                    isAttacking = true;
                    lastAttackTime = Time.time;
                }
                else if (distanceToPlayer > minAreaAttackDistance && distanceToPlayer <= areaJumpMaxDistance)
                {
                    isAttacking = true;
                    lastAttackTime = Time.time;
                    StartCoroutine(PrepareAreaAttack());
                }
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private IEnumerator PrepareAreaAttack()
    {
        float originalSpeed = speed;
        speed = 0f;
        animator.SetBool("isMoving", false);
        yield return new WaitForSeconds(0.2f);

        animator.SetTrigger("AreaAttack");
        yield return new WaitForSeconds(0.05f);

        yield return new WaitUntil(() => !isAreaJumping);
        speed = originalSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isAreaJumping) return;

        DoDamage(collision);

        if (collision.CompareTag("Player") && !isDead)
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null && !playerMovement.IsRolling && !isAttacking)
            {
                animator.SetTrigger("Attack");
                isAttacking = true;
                lastAttackTime = Time.time;
            }
        }
    }

    public void DealDamageToPlayer()
    {
        if (isDead) return;

        if (playerTransform != null)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RegisterAttacker(gameObject);
                playerHealth.TakeDamage(damage);
            }
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    private void DoDamage(Collider2D collision)
    {
        if (collision.transform.root == transform)
            return;

        if (collision.CompareTag("Bullet"))
        {
            TakeDamage(1);
        }
        else if (collision.CompareTag("HolyBullet"))
        {
            TakeDamage(5);
        }
    }


    private void TakeDamage(int amount)
    {
        if (!isInvulnerable && !isDead)
        {
            audioSource.PlayOneShot(GethitSound());
            currentHp -= amount;

            if (bossHealthSlider != null)
            {
                bossHealthSlider.value = currentHp;
            }

            if (hitEffect != null)
            {
                hitEffect.gameObject.SetActive(true);
                hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                hitEffect.Play();
            }

            if (currentHp <= 0)
            {
                StartCoroutine(Death());
            }
            else
            {
                StartCoroutine(DamageFlashAndInvulnerability());
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Building"))
        {
            Vector2 awayFromBuilding = (transform.position - collision.transform.position).normalized;
            avoidanceDirection = awayFromBuilding;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Building"))
        {
            avoidanceDirection = Vector2.zero;
        }
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        isInvulnerable = true;
        spriteRenderer.color = Color.red;
        myCollider.enabled = false;
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
        myCollider.enabled = true;
    }

    private IEnumerator Death()
    {
        isDead = true;
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
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (hitEffect != null)
            yield return new WaitForSeconds(hitEffect.main.duration);

        gameObject.SetActive(false);
    }

    private AudioClip GethitSound()
    {
        int random = Random.Range(0, 3);
        return random switch
        {
            0 => hit1,
            1 => hit2,
            2 => hit3,
            _ => null
        };
    }

    public void Celebrate()
    {
        if (isDead || celebrating) return;

        celebrating = true;
        isAttacking = false;
        isAreaJumping = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        animator.SetBool("isMoving", false);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("AreaAttack");
        animator.SetTrigger("Celebration");
    }

    public void AreaAttack_Begin()
    {
        if (isDead || playerTransform == null) return;

        jumpStartPos = rb.position;
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        float dist = Mathf.Min(Vector2.Distance(playerTransform.position, transform.position), areaJumpMaxDistance);

        dist *= (1f - landingOffsetFactor);
        jumpEndPos = jumpStartPos + dir * dist;

        isAreaJumping = true;
        isAttacking = true;
        jumpElapsed = 0f;
    }

    private void ExecuteAreaImpact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaImpactRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.RegisterAttacker(gameObject);
                    playerHealth.TakeDamage(damage * 2);
                }
            }
        }
    }
}
