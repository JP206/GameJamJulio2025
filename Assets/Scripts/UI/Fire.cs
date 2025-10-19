using UnityEngine;

public class Fire : MonoBehaviour
{
    [SerializeField] private int damage = 2;
    [SerializeField] private float damageInterval = 1f;

    private float nextDamageTime = 0f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Time.time < nextDamageTime) return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                nextDamageTime = Time.time + damageInterval;
            }
        }
        else if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            EnemyController enemyController = collision.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                var takeDamageMethod = typeof(EnemyController).GetMethod("TakeDamage",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (takeDamageMethod != null)
                {
                    takeDamageMethod.Invoke(enemyController, new object[] { damage });
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
    }
}
