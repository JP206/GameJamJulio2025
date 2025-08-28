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
        IgnoreAllAmmoCollisions();
    }

    private void Disable()
    {
        BulletPool.Instance.ReturnBullet(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ammo")) return;

        BulletPool.Instance.ReturnBullet(gameObject);
    }

    private void IgnorePlayerCollision()
    {
        var player = GameObject.FindWithTag("Player");
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
        var confiner = GameObject.Find("CameraConfiner");
        if (confiner != null)
        {
            var confCol = confiner.GetComponent<Collider2D>();
            if (confCol != null)
            {
                Physics2D.IgnoreCollision(
                    GetComponent<Collider2D>(),
                    confCol,
                    true
                );
            }
        }
    }

    private void IgnoreAllAmmoCollisions()
    {
        var myCol = GetComponent<Collider2D>();
        var ammos = GameObject.FindGameObjectsWithTag("Ammo");
        foreach (var a in ammos)
        {
            var col = a.GetComponent<Collider2D>();
            if (col != null) Physics2D.IgnoreCollision(myCol, col, true);
        }
    }
}
