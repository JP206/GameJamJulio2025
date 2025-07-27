using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f, bulletAmmo;
    [SerializeField] TextMeshProUGUI ammoText, ammoTextShade;
    [SerializeField] AudioClip gunshot1, emptyGunshot, eat1, eat2, eat3;
    [SerializeField] AudioSource audioSource1, audioSource2;
    [SerializeField] float chickenRangeOrigin = 0.2f, chickenRangeEnd = 0.4f;
    Animator animator;
    UIAnimation uiAnimation;
    PlayerMovement playerMovement;
    [SerializeField] ParticleSystem shootParticlesLeft, shootParticlesRight;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        animator = GetComponent<Animator>();

        uiAnimation = FindAnyObjectByType<UIAnimation>();

        playerMovement = GetComponent<PlayerMovement>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        shootParticlesLeft.Stop();
        shootParticlesRight.Stop();
    }

    private void Update()
    {
        if (UICanvasManager.IsGamePausedOrOver) return;
        if (IntroCinematicManager.IsCinematicPlaying) return;

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

            float randomScale = Random.Range(chickenRangeOrigin, chickenRangeEnd);
            bullet.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 fireDirection = (mousePos - (Vector2)firePoint.position).normalized;

            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = fireDirection * bulletSpeed;

            audioSource1.PlayOneShot(gunshot1);

            if (animator.GetBool("isRunning"))
            {
                if (playerMovement.GetFacingForward())
                {
                    animator.SetTrigger("fire");
                }
                else
                {
                    animator.SetTrigger("FireBackwards");
                }
            }

            if (!spriteRenderer.flipX)
            {
                shootParticlesRight.Stop();
                shootParticlesRight.Play();
            }
            else
            {
                shootParticlesLeft.Stop();
                shootParticlesLeft.Play();
            }
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
            ammoTextShade.text = ammoText.text;
            uiAnimation.Animation(ammoText, ammoTextShade);

            audioSource2.PlayOneShot(GetEatSound());
        }
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
