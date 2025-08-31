using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class _GunController : MonoBehaviour
{
    [SerializeField] private Transform firePoint; 
    [SerializeField] private float bulletSpeed = 15f, bulletAmmo; 
    [SerializeField] private float maxAngle = 45f; 
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] TextMeshProUGUI ammoTextShade;
    [SerializeField] AudioClip emptyGunshot, eat1, eat2, eat3; 
    [SerializeField] AudioSource audioSource1, audioSource2, shotgunAudioSource; 
    [SerializeField] float chickenRangeOrigin = 0.2f, chickenRangeEnd = 0.4f; 
    [SerializeField] ParticleSystem shootParticlesLeft, shootParticlesRight;

    SpriteRenderer spriteRenderer;
    PlayerMovement playerMovement;
    Animator animator; UIAnimation uiAnimation;

    private float nextFireTime = 0f;

    private void Start() 
    {
        animator = GetComponent<Animator>(); 
        uiAnimation = FindAnyObjectByType<UIAnimation>(); 
        playerMovement = GetComponent<PlayerMovement>(); 
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        shootParticlesLeft.Stop(); 
        shootParticlesRight.Stop();
    }
    private void Update() { 
        if (UICanvasManager.IsGamePausedOrOver) return; 
        if (IntroCinematicManager.IsCinematicPlaying) return;
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            FireBullet();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void FireBullet()
    {
        PlayShotgunSound(shotgunAudioSource);

        if (bulletAmmo > 0)
        {
            bulletAmmo--; ammoText.text = "Ammo: " + bulletAmmo.ToString(); 
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

            if (!spriteRenderer.flipX) 
            { 
                shootParticlesRight.Stop(); 
                shootParticlesRight.Play(); 
            }
            else 
            { 
                shootParticlesLeft.Stop(); 
                shootParticlesLeft.Play(); } 
        } 
        else { 
            audioSource1.PlayOneShot(emptyGunshot); 
        } 
    }
    private void OnTriggerEnter2D(Collider2D collision) 
    { 
        if (collision.CompareTag("Ammo")) 
        { 
            bulletAmmo += 10; ammoText.text = "Ammo: " + bulletAmmo.ToString(); 
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
            case 0: return eat1; 
            case 1: return eat2; 
            case 2: return eat3; 
        } 
        return null; 
    }

    private void PlayShotgunSound(AudioSource source)
    {
        if (source != null && source.clip != null)
        {
            source.PlayOneShot(source.clip);
        }
    }
}
