using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float lifeTime = 2f;

    private void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(Disable), lifeTime);

        IgnorePlayerCollision();
        IgnoreConfinerCollision();
    }

    private void Disable()
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }

    private void IgnorePlayerCollision()
    {
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

    private void IgnoreConfinerCollision()
    {
        GameObject confiner = GameObject.Find("CameraConfiner");
        if (confiner != null)
        {
            Collider2D confinerCol = confiner.GetComponent<Collider2D>();
            if (confinerCol != null)
            {
                Physics2D.IgnoreCollision(
                    GetComponent<Collider2D>(),
                    confinerCol,
                    true
                );
            }
        }
    }
}
