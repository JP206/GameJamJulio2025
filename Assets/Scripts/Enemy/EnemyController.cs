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

            if (animator != null && HasParameter(animator, "isMoving"))
            {
                animator.SetBool("isMoving", movementMagnitude > 0.01f);
            }

            rb.MovePosition(newPosition);

            float xDiff = transform.position.x - playerTransform.position.x;
            spriteRenderer.flipX = xDiff < 0;
        }
        else
        {
            if (animator != null && HasParameter(animator, "isMoving"))
            {
                animator.SetBool("isMoving", false);
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
                    {
                        animator.SetTrigger("Attack");
                    }
                    playerHealth.TakeDamage(damage);
                }
            }
        }
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
        collider.enabled = false;

        if (bossChicken && animator != null && HasParameter(animator, "GetHit"))
        {
            animator.SetTrigger("GetHit");
        }

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
        {
            waveManager.NotifyDeath();
        }

        if (animator != null && HasParameter(animator, "Death"))
        {
            animator.SetTrigger("Death");
        }

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
        yield return new WaitForSeconds(animator != null ? animator.GetCurrentAnimatorStateInfo(0).length : 0f);

        if (hitEffect != null)
        {
            yield return new WaitForSeconds(hitEffect.main.duration);
        }

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
        {
            animator.SetTrigger("Celebration");
        }
    }

    private void SetBossAsTrue()
    {
        if (gameObject.CompareTag("Boss")) bossChicken = true;
    }

    // Helper para evitar warnings si faltan parámetros en el Animator
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
