using System.Collections;
using UnityEngine;

public class PolloLocoController : MonoBehaviour
{
    [SerializeField] float speed, invulnerabilityTime, flashInterval;
    [SerializeField] int damage, maxHp, currentHp;
    [SerializeField] float attackRange;
    [SerializeField] float attackCooldown;
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
            Vector2 direction = (playerTransform.position - transform.position).normalized;

            if (avoidanceDirection != Vector2.zero)
            {
                direction = Vector2.Lerp(direction, avoidanceDirection, 0.5f);
            }

            if (direction.magnitude < 0.1f)
            {
                direction = (playerTransform.position - transform.position).normalized;
            }

            direction = direction.normalized;
            Vector2 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;

            float movementMagnitude = (newPosition - rb.position).magnitude;
            animator.SetBool("isMoving", movementMagnitude > 0.01f);

            rb.MovePosition(newPosition);

            float xDiff = transform.position.x - playerTransform.position.x;
            spriteRenderer.flipX = xDiff < 0;

            float distanceToPlayer = Vector2.Distance(playerTransform.position, transform.position);
            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown && !isAttacking)
            {
                animator.SetTrigger("Attack");
                isAttacking = true;
                lastAttackTime = Time.time;
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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
        waveManager.NotifyDeath();
        animator.SetTrigger("Death");

        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(true);
            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitEffect.Play();
        }

        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (hitEffect != null)
        {
            yield return new WaitForSeconds(hitEffect.main.duration);
        }

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
        celebrating = true;
        animator.SetTrigger("Celebration");
    }
}
