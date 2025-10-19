using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class _GunController : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 15f, bulletAmmo;
    [SerializeField] private float maxAngle = 45f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI ammoTextShade;
    [SerializeField] private AudioClip emptyGunshot, eat1, eat2, eat3;
    [SerializeField] private AudioSource audioSource1, audioSource2, shotgunAudioSource;
    [SerializeField] private float chickenRangeOrigin = 0.2f, chickenRangeEnd = 0.4f;
    [SerializeField] private ParticleSystem shootParticlesLeft, shootParticlesRight;
    [SerializeField] private ParticleSystem bloomParticleRight, lightRayParticleRight;
    [SerializeField] private ParticleSystem bloomParticleLeft, lightRayParticleLeft;

    [Header("Holy Bullet Settings")]
    [SerializeField] private ParticleSystem holyAuraPS;
    [SerializeField] private GameObject holyBulletPrefab;
    [SerializeField] private Transform holyBulletSpawnPointRight;
    [SerializeField] private Transform holyBulletSpawnPointLeft;
    [SerializeField] private AudioSource holyShotSource;

    SpriteRenderer spriteRenderer;
    PlayerMovement playerMovement;
    Animator animator;
    UIAnimation uiAnimation;

    private float nextFireTime = 0f;
    private bool isChargingHolyShot = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        uiAnimation = FindAnyObjectByType<UIAnimation>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        shootParticlesLeft.Stop();
        shootParticlesRight.Stop();
        bloomParticleRight.Stop();
        lightRayParticleRight.Stop();
        bloomParticleLeft.Stop();
        lightRayParticleLeft.Stop();
        holyAuraPS.Stop();
    }

    private void Update()
    {
        if (UICanvasManager.IsGamePausedOrOver) return;
        if (IntroCinematicManager.IsCinematicPlaying) return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime && !isChargingHolyShot)
        {
            FireBullet();
            nextFireTime = Time.time + fireRate;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && !isChargingHolyShot)
        {
            StartCoroutine(FireHolyShot());
        }
    }

    private void FireBullet()
    {
        if (bulletAmmo > 0)
        {
            PlayShotgunSound(shotgunAudioSource);

            bulletAmmo--;
            if (GameManager.Instance != null)
                GameManager.Instance.playerAmmo = bulletAmmo;

            ammoText.text = "Ammo: " + bulletAmmo.ToString();
            ammoTextShade.text = ammoText.text;

            GameObject bullet = BulletPool.Instance.GetBullet();
            bullet.transform.position = firePoint.position;

            float randomScale = Random.Range(chickenRangeOrigin, chickenRangeEnd);
            bullet.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rawDirection = (mousePos - (Vector2)firePoint.position).normalized;
            Vector2 forward = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            float angleToMouse = Vector2.SignedAngle(forward, rawDirection);
            angleToMouse = Mathf.Clamp(angleToMouse, -maxAngle, maxAngle);

            Quaternion rotation = Quaternion.AngleAxis(angleToMouse, Vector3.forward);
            Vector2 clampedDirection = rotation * forward;

            float finalAngle = Mathf.Atan2(clampedDirection.y, clampedDirection.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            Vector2 playerVel = playerMovement.GetVelocity();
            float projection = Vector2.Dot(playerVel, clampedDirection);

            if (projection > 0) rb.linearVelocity = clampedDirection * (bulletSpeed + projection);
            else rb.linearVelocity = clampedDirection * bulletSpeed;

            if (animator.GetBool("isRunning") || animator.GetBool("isRunningBackwards"))
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
            else
            {
                animator.SetTrigger("StaticFire");
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

    private IEnumerator FireHolyShot()
    {
        isChargingHolyShot = true;
        animator.SetTrigger("HolyShot");
        playerMovement.SetHolyShotState(true);

        yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("HolyShot"))
            yield return null;

        playerMovement.SetHolyShotState(false);
        isChargingHolyShot = false;
    }

    public void FireHolyBullet()
    {
        Transform spawnPoint = spriteRenderer.flipX ? holyBulletSpawnPointLeft : holyBulletSpawnPointRight;
        GameObject bullet = Instantiate(holyBulletPrefab, spawnPoint.position, Quaternion.identity);

        Vector2 dir = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        bullet.GetComponent<HolyBullet>().SetDirection(dir);

        if (spriteRenderer.flipX)
        {
            Vector3 scale = bullet.transform.localScale;
            scale.x *= -1;
            bullet.transform.localScale = scale;
        }
    }

    public void PlayHolyShotParticle()
    {
        if (!spriteRenderer.flipX)
        {
            bloomParticleRight.Play();
            lightRayParticleRight.Play();
        }
        else
        {
            bloomParticleLeft.Play();
            lightRayParticleLeft.Play();
        }
    }

    public void StopHolyShotParticle()
    {
        bloomParticleRight.Stop();
        lightRayParticleRight.Stop();
        bloomParticleLeft.Stop();
        lightRayParticleLeft.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ammo"))
        {
            bulletAmmo += 10;
            ammoText.text = "Ammo: " + bulletAmmo.ToString();
            ammoTextShade.text = ammoText.text;

            if (GameManager.Instance != null)
                GameManager.Instance.playerAmmo = bulletAmmo;

            uiAnimation.Animation(ammoText, ammoTextShade);
            audioSource2.PlayOneShot(GetEatSound());
        }
    }

    private AudioClip GetEatSound()
    {
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0: return eat1;
            case 1: return eat2;
            case 2: return eat3;
        }
        return null;
    }

    public float GetAmmo()
    {
        return bulletAmmo;
    }

    public void SetAmmo(float amount)
    {
        bulletAmmo = amount;

        if (ammoText != null)
            ammoText.text = "Ammo: " + bulletAmmo.ToString();

        if (ammoTextShade != null)
            ammoTextShade.text = ammoText.text;

        if (GameManager.Instance != null)
            GameManager.Instance.playerAmmo = bulletAmmo;
    }

    private void PlayShotgunSound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.PlayOneShot(source.clip);
        }
    }

    private void PlayHolySound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.PlayOneShot(source.clip);
        }
    }

    public void PlayAura()
    {
        if (holyAuraPS != null) holyAuraPS.Play();
        PlayHolySound(holyShotSource);
    }

    public void StopAura()
    {
        if (holyAuraPS != null) holyAuraPS.Stop();
    }

    public void PlayShotgunSoundEvent()
    {
        PlayShotgunSound(shotgunAudioSource);
    }

    public void RefreshAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = "Ammo: " + bulletAmmo.ToString();
        }

        if (ammoTextShade != null)
        {
            ammoTextShade.text = ammoText.text;
        }
    }
}
