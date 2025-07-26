using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float speed, invulnerabilityTime = 2f, flashInterval = 0.1f;
    [SerializeField] int damage, maxHp, currentHp;

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false, isDead = false;
    private Animator animator;
    private WaveManager waveManager;
    private Collider2D collider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerTransform = player.transform;
        }
    }

    private void OnEnable()
    {
        isDead = false;
        currentHp = maxHp;
    }

    public void SetWaveManager(WaveManager _waveManager)
    {
        waveManager = _waveManager;
    }

    void Update()
    {
        if (playerTransform && !isDead)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed * Time.deltaTime);
            
            float distanceToPlayer = transform.position.x - playerTransform.position.x;
            spriteRenderer.flipX = distanceToPlayer < 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            if (!isInvulnerable && !isDead)
            {
                currentHp--;

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

        if (collision.CompareTag("Player"))
        {
            if (!isDead)
            {
                PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    animator.SetTrigger("Attack");
                    playerHealth.TakeDamage(damage);
                }
            }
        }
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        isInvulnerable = true;

        spriteRenderer.color = Color.red;
        collider.enabled = false;
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
        waveManager.NotifyDeath();
        animator.SetTrigger("Death");

        yield return null;

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        yield return new WaitForSeconds(0.2f);

        gameObject.SetActive(false);
    }
}
