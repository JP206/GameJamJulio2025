using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityTime = 2f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource lifeUpSound;
    [SerializeField] private AudioSource deathSound;
    [SerializeField] private AudioClip hit1, hit2, hit3;
    [SerializeField] private GameObject deathEffectPrefab;

    private int currentHealth;
    private bool isInvulnerable = false;
    private SpriteRenderer _spriteRenderer;
    private Animator animator;

    public UnityEvent<int, int> OnHealthChanged;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (OnHealthChanged == null)
            OnHealthChanged = new UnityEvent<int, int>();

        Debug.Log("PlayerHealth Awake → maxHealth = " + maxHealth);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Log("PlayerHealth Start → Animator asignado: " + (animator != null));
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable)
        {
            Debug.Log("TakeDamage bloqueado → invulnerable activo");
            return;
        }

        audioSource.PlayOneShot(GethitSound());
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("TakeDamage → daño recibido = " + amount + " | HP restante = " + currentHealth);

        animator.SetTrigger("GetHit");
        Debug.Log("Trigger GetHit lanzado");

        OnHealthChanged.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("HP llegó a 0 → ejecutando muerte");

            if (deathEffectPrefab != null)
            {
                PlayDeathSound(deathSound);
                GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }

            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            foreach (EnemyController enemy in enemies)
            {
                Debug.Log("Notificando a enemigo para celebrar: " + enemy.name);
                enemy.Celebrate();
            }

            UICanvasManager uiManager = Object.FindFirstObjectByType<UICanvasManager>();
            if (uiManager != null)
            {
                Debug.Log("Disparando Game Over en UI");
                uiManager.TriggerGameOver();
            }

            StartCoroutine(Death());
            return;
        }

        StartCoroutine(DamageFlashAndInvulnerability());
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        isInvulnerable = true;
        Debug.Log("Entrando en invulnerabilidad");

        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        float elapsed = 0f;
        while (elapsed < invulnerabilityTime)
        {
            _spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashInterval / 2f);
            _spriteRenderer.color = Color.clear;
            yield return new WaitForSeconds(flashInterval / 2f);
            elapsed += flashInterval;
        }

        _spriteRenderer.color = Color.white;
        isInvulnerable = false;
        Debug.Log("Invulnerabilidad terminada");
    }

    private AudioClip GethitSound()
    {
        int random = Random.Range(0, 3);
        Debug.Log("Reproduciendo sonido de hit #" + random);

        switch (random)
        {
            case 0: return hit1;
            case 1: return hit2;
            case 2: return hit3;
        }
        return null;
    }

    private IEnumerator Death()
    {
        Debug.Log("Corrutina Death iniciada");
        yield return new WaitForSeconds(0.1f);
        Debug.Log("GameObject Player desactivado");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Life"))
        {
            Debug.Log("Colisión con Life → curando");

            PlayLifeUpSound(lifeUpSound);

            int healAmount = Mathf.RoundToInt(maxHealth * 0.15f);
            currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, maxHealth);

            OnHealthChanged.Invoke(currentHealth, maxHealth);

            Debug.Log("Curado → HP actual = " + currentHealth);
        }
    }

    private void PlayLifeUpSound(AudioSource lifeUpSound)
    {
        if (lifeUpSound != null && lifeUpSound.clip != null)
        {
            Debug.Log("Reproduciendo sonido de vida extra");
            lifeUpSound.PlayOneShot(lifeUpSound.clip);
        }
    }

    private void PlayDeathSound(AudioSource deathSound)
    {
        if (deathSound != null && deathSound.clip != null)
        {
            Debug.Log("Reproduciendo sonido de muerte");
            AudioSource.PlayClipAtPoint(deathSound.clip, transform.position);
        }
    }
}
