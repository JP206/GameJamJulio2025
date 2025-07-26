using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float lifeTime = 3f;

    private void OnEnable()
    {
        CancelInvoke();
        Invoke("Disable", lifeTime);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Physics2D.IgnoreCollision(
                GetComponent<Collider2D>(),
                player.GetComponent<Collider2D>(),
                true
            );
        }
    }

    private void Disable()
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }
}
