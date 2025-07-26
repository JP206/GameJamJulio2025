using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f, bulletAmmo;
    [SerializeField] TextMeshProUGUI ammoText;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            FireBullet();
        }
    }

    private void FireBullet()
    {
        if (bulletAmmo > 0)
        {
            bulletAmmo--;
            ammoText.text = "Ammo: " + bulletAmmo.ToString();

            GameObject bullet = BulletPool.Instance.GetBullet();
            bullet.transform.position = firePoint.position;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 fireDirection = (mousePos - (Vector2)firePoint.position).normalized;

            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = fireDirection * bulletSpeed;
        }
        else
        {
            // sonido de que no puede disparar
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ammo"))
        {
            bulletAmmo += 10;
            ammoText.text = "Ammo: " + bulletAmmo.ToString();
        }
    }
}
