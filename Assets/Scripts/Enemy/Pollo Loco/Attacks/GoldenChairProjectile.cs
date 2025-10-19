using UnityEngine;

public class GoldenChairProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 8;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float knockbackForce = 14f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var health = collision.GetComponent<PlayerHealth>();
            var rb = collision.GetComponent<Rigidbody2D>();

            if (health != null)
            {
                health.TakeDamage(damage);
                Vector2 knockDir = (rb.transform.position - transform.position).normalized;
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            Destroy(gameObject);
        }
    }
}
