using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f, bulletAmmo;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] AudioClip gunshot1, gunshot2, gunshot3, emptyGunshot, eat1, eat2, eat3;
    [SerializeField] AudioSource audioSource1, audioSource2;


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

            audioSource1.PlayOneShot(GetShotSound());
        }
        else
        {
            audioSource1.PlayOneShot(emptyGunshot);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ammo"))
        {
            bulletAmmo += 10;
            ammoText.text = "Ammo: " + bulletAmmo.ToString();

            audioSource2.PlayOneShot(GetEatSound());
        }
    }

    private AudioClip GetShotSound()
    {
        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                return gunshot1;
            case 1:
                return gunshot2;
            case 2:
                return gunshot3;
        }

        return null;
    }

    private AudioClip GetEatSound()
    {
        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                return eat1;
            case 1:
                return eat2;
            case 2:
                return eat3;
        }

        return null;
    }
}
