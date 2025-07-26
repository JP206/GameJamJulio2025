using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float speed, invulnerabilityTime = 2f, flashInterval = 0.1f;
    [SerializeField] int damage, hp;

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private bool isInvulnerable = false;
    private Animator animator;
    private WaveManager waveManager;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerTransform = player.transform;
        }
    }

    public void SetWaveManager(WaveManager _waveManager)
    {
        waveManager = _waveManager;
    }

    void Update()
    {
        if (playerTransform)
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
            if (!isInvulnerable)
            {
                hp--;

                if (hp <= 0)
                {
                    waveManager.NotifyDeath();
                    gameObject.SetActive(false);
                }
                else
                {
                    StartCoroutine(DamageFlashAndInvulnerability());
                }
            }
        }

        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                animator.SetTrigger("Attack");
                playerHealth.TakeDamage(damage);
            }
        }
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        isInvulnerable = true;

        spriteRenderer.color = Color.red;
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
    }
}
