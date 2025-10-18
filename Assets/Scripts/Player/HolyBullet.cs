using UnityEngine;
using System.Collections;

public class HolyBullet : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string travelStateName = "Travel";
    [SerializeField] private string explosionTrigger = "Explode";

    [Header("Optional Components")]
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 direction;
    private bool hasExploded = false;

    private void Reset()
    {
        animator = GetComponent<Animator>();
        hitCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        hasExploded = false;

        // Reinicia animaci�n al activarse (clave si us�s object pooling)
        if (animator != null && !string.IsNullOrEmpty(travelStateName))
            animator.Play(travelStateName, 0, 0f);

        // Reactiva colisi�n
        if (hitCollider != null)
            hitCollider.enabled = true;

        // Reinicia movimiento
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        CancelInvoke();
        Invoke(nameof(TimeoutExplosion), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        if (!hasExploded)
        {
            if (rb != null)
                rb.linearVelocity = direction * speed;
            else
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }

    private void TimeoutExplosion()
    {
        if (!hasExploded)
            TriggerExplosion();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasExploded && other.CompareTag("Building"))
        {
            TriggerExplosion();
        }
    }

    private void TriggerExplosion()
    {
        hasExploded = true;
        CancelInvoke();

        // Frenar movimiento
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Evitar m�ltiples colisiones
        if (hitCollider != null)
            hitCollider.enabled = false;

        // Activar animaci�n de explosi�n
        if (animator != null && !string.IsNullOrEmpty(explosionTrigger))
            animator.SetTrigger(explosionTrigger);
        else
            gameObject.SetActive(false); // fallback si no hay animator
    }

    // === Animation Event ===
    // Agreg� este evento en el �ltimo frame de "Holy Blast"
    public void OnExplosionEnd()
    {
        gameObject.SetActive(false);
    }
}
