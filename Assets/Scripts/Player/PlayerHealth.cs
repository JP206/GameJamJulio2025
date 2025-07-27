using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityTime = 2f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hit1, hit2, hit3;
    [SerializeField] private GameObject deathEffectPrefab;

    private int currentHealth;
    private bool isInvulnerable = false;
    private SpriteRenderer _spriteRenderer;

    public UnityEvent<int, int> OnHealthChanged;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (OnHealthChanged == null)
            OnHealthChanged = new UnityEvent<int, int>();
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        audioSource.PlayOneShot(GethitSound());
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            if (deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }

            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            foreach (EnemyController enemy in enemies)
            {
                enemy.Celebrate();
            }

            UICanvasManager uiManager = Object.FindFirstObjectByType<UICanvasManager>();
            if (uiManager != null)
                uiManager.TriggerGameOver();

            StartCoroutine(Death());
            return;
        }

        StartCoroutine(DamageFlashAndInvulnerability());
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        isInvulnerable = true;

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

    private IEnumerator Death()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
}
