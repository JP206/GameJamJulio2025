using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnerabilityTime = 2f;
    [SerializeField] private float flashInterval = 0.1f;

    private int currentHealth;
    private bool isInvulnerable = false;
    private SpriteRenderer spriteRenderer;

    public UnityEvent<int, int> OnHealthChanged; // (currentHealth, maxHealth)

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (OnHealthChanged == null)
            OnHealthChanged = new UnityEvent<int, int>();
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(DamageFlashAndInvulnerability());
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
