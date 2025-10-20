using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PolloLocoController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public float speed = 4f;
    [SerializeField] public float invulnerabilityTime = 2f;
    [SerializeField] public float flashInterval = 0.1f;
    [SerializeField] public int damage = 5;
    [SerializeField] public int maxHp = 50;
    [SerializeField] public int currentHp;
    [SerializeField] public float attackRange = 4f;
    [SerializeField] public float attackCooldown = 0.6f;

    [Header("Audio & FX")]
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip hit1;
    [SerializeField] public AudioClip hit2;
    [SerializeField] public AudioClip hit3;
    [SerializeField] public ParticleSystem hitEffect;

    [Header("UI")]
    [SerializeField] public Slider bossHealthSlider;

    [Header("Cinematic Control")]
    [SerializeField] public bool isCinematicMode = false;

    [Header("Boss Behavior")]
    [SerializeField] public float detectionRange = 15f;
    [SerializeField] public float closeAttackRange = 1.5f;
    [SerializeField] public float comboPause = 0.25f;
    [SerializeField] public float postDashDelay = 0.4f;

    [Header("Knockback Settings")]
    [SerializeField] public float normalPushForce = 8f;
    [SerializeField] public float attackPushForce = 14f;
    [SerializeField] public float knockbackDuration = 0.25f;

    [Header("Attack Decision Settings")]
    [SerializeField] public float meleeRange = 2.5f;
    [SerializeField] public float rangeAttackDistance = 6f;
    [SerializeField] public float areaAttackDistance = 10f;
    [SerializeField] public float dashSpeed = 20f;
    [SerializeField] public float dashDuration = 0.18f;
    [SerializeField] public float areaAttackCooldown = 4f;
    [SerializeField] public float areaDamageRadius = 3.5f;
    [SerializeField] private ParticleSystem plumaImpactEffect;

    [Header("Counter Dash Settings")]
    [SerializeField] public int hitsToTriggerDash = 2;
    [SerializeField] public float hitResetTime = 1f;
    [SerializeField] public float counterDashMultiplier = 1.2f;
    [SerializeField] public float counterDashRecovery = 0.7f;

    [Header("Chair Attack Settings")]
    [SerializeField] public float chairAttackDistance = 15f;
    [SerializeField] public float chairAttackCooldown = 10f;
    [SerializeField] public GameObject chairPrefab;
    [SerializeField] public float chairThrowForce = 15f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip throwChairClip;
    [SerializeField] private float throwChairVolume = 1f;
    [SerializeField] private AudioClip animationEventClip;
    [SerializeField] private float animationEventVolume = 1f;

    private Transform playerTransform;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D myCollider;
    private Rigidbody2D rb;
    private PolloLocoAttackHandler attackHandler;

    private bool isInvulnerable = false;
    private bool isDead = false;
    private bool celebrating = false;
    private bool isAttacking = false;
    private bool isDashing = false;
    private bool canDealDamage = false;
    private bool hasAttackedInRange = false;

    [SerializeField] private int hitsWhileClose = 0;
    [SerializeField] private float lastAttackTime;
    [SerializeField] private float lastDashTime;
    [SerializeField] private float lastAreaAttackTime;
    [SerializeField] private float lastChairAttackTime;
    [SerializeField] private float lastHitTime;
    [SerializeField] private Vector2 lastKnownPlayerPos;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        attackHandler = GetComponent<PolloLocoAttackHandler>();

        if (attackHandler != null)
            attackHandler.Initialize(animator, rb, myCollider, this);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
            playerTransform = player.transform;

        if (hitEffect == null)
            hitEffect = GetComponentInChildren<ParticleSystem>();
    }

    private void OnEnable()
    {
        isDead = false;
        currentHp = maxHp;
        lastAttackTime = -attackCooldown;
        lastDashTime = -2f;
        lastAreaAttackTime = -areaAttackCooldown;
        lastChairAttackTime = -chairAttackCooldown;
        isAttacking = false;
        isDashing = false;

        if (hitEffect != null)
            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (bossHealthSlider != null)
        {
            bossHealthSlider.maxValue = maxHp;
            bossHealthSlider.value = currentHp;
        }
    }

    void FixedUpdate()
    {
        if (isAttacking || isDashing) return;

        if (isCinematicMode)
        {
            animator.SetBool("isMoving", false);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerTransform == null || isDead || celebrating) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        Vector2 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;

        if (!isDashing && !isAttacking)
        {
            rb.MovePosition(newPosition);
            animator.SetBool("isMoving", true);
        }

        spriteRenderer.flipX = (transform.position.x - playerTransform.position.x) < 0;
        lastKnownPlayerPos = playerTransform.position;

        float distanceToPlayer = Vector2.Distance(playerTransform.position, transform.position);

        if (hitsWhileClose > 0 && Time.time - lastHitTime > hitResetTime)
            hitsWhileClose = 0;

        // ====== DECISIÓN DE ATAQUE ======
        if (!isAttacking && !isDashing && Time.time >= lastAttackTime + attackCooldown)
        {
            // --- Melee ---
            if (distanceToPlayer <= closeAttackRange && Time.time >= lastDashTime + postDashDelay)
            {
                StartCoroutine(DoCloseAttack());
            }
            // --- Dash ---
            else if (distanceToPlayer > closeAttackRange &&
                     distanceToPlayer <= rangeAttackDistance &&
                     Time.time >= lastDashTime + postDashDelay)
            {
                StartCoroutine(DoInstantDashAttack());
            }
            // --- Area ---
            else if (distanceToPlayer > rangeAttackDistance &&
                     distanceToPlayer <= areaAttackDistance &&
                     Time.time >= lastAreaAttackTime + areaAttackCooldown &&
                     !hasAttackedInRange)
            {
                hasAttackedInRange = true;
                StartCoroutine(DoAreaAttack());
            }
            // --- Chair Attack ---
            else if (distanceToPlayer > areaAttackDistance &&
                     distanceToPlayer <= chairAttackDistance &&
                     Time.time >= lastChairAttackTime + chairAttackCooldown)
            {
                StartCoroutine(DoChairAttack());
            }

            // Reset de la flag cuando el jugador sale del rango total del área
            if (distanceToPlayer > areaAttackDistance)
            {
                hasAttackedInRange = false;
            }
        }
    }

    private IEnumerator DoCloseAttack()
    {
        if (playerTransform == null || isDead || attackHandler == null) yield break;
        isAttacking = true;
        yield return StartCoroutine(attackHandler.MeleeAttack(OnAttackEnd));
    }

    private IEnumerator DoInstantDashAttack()
    {
        if (isDead || isAttacking || playerTransform == null || attackHandler == null) yield break;
        isAttacking = true;
        isDashing = true;
        lastDashTime = Time.time;
        Vector2 dashDir = (lastKnownPlayerPos - (Vector2)transform.position).normalized;
        yield return StartCoroutine(attackHandler.DashAttack(dashDir, OnAttackEnd));
    }

    private IEnumerator DoCounterDashAttack()
    {
        if (isDead || isAttacking || playerTransform == null || attackHandler == null) yield break;
        isAttacking = true;
        isDashing = true;
        lastDashTime = Time.time;
        Vector2 dashDir = (lastKnownPlayerPos - (Vector2)transform.position).normalized;
        yield return StartCoroutine(attackHandler.CounterDash(dashDir, OnAttackEnd));
    }

    private IEnumerator DoAreaAttack()
    {
        if (isDead || isAttacking || isDashing || playerTransform == null || attackHandler == null)
        {
            yield break;
        }

        // 🔹 Entramos en modo ataque
        isAttacking = true;
        isDashing = true;
        hasAttackedInRange = true;

        // 🔹 Capturamos la posición actual del jugador justo antes del salto
        lastKnownPlayerPos = playerTransform.position;
        Vector2 jumpDir = (lastKnownPlayerPos - (Vector2)transform.position).normalized;

        // 🔹 Ejecutamos el salto (sin depender del evento)
        yield return StartCoroutine(attackHandler.AreaAttack(jumpDir, null));

        // 🔹 Esperamos el tiempo de la animación completa (seguridad)
        yield return new WaitForSeconds(0.5f);

        // 🔹 Reiniciamos los flags aunque el Animator no haya avisado
        isAttacking = false;
        isDashing = false;
        lastAreaAttackTime = Time.time;

    }



    // === NUEVO ATAQUE DE SILLA ===
    private IEnumerator DoChairAttack()
    {
        if (isDead || isAttacking || isDashing || attackHandler == null) yield break;

        isAttacking = true;
        lastChairAttackTime = Time.time;

        animator.SetTrigger("Chair");

        yield return new WaitForSeconds(1f); // duración total de la animación
        OnAttackEnd();
    }
    public void ThrowChair()
    {
        if (chairPrefab == null || playerTransform == null) return;

        Vector2 startPos = transform.position;
        Vector2 targetPos = playerTransform.position;

        // Dirección general
        Vector2 direction = (targetPos - startPos).normalized;

        // Aumentamos fuerza y agregamos un leve ángulo de elevación
        Vector2 launchDir = (direction + Vector2.up * 0.25f).normalized;

        GameObject chair = Instantiate(chairPrefab, startPos, Quaternion.identity);
        Rigidbody2D chairRb = chair.GetComponent<Rigidbody2D>();

        if (chairRb != null)
        {
            chairRb.bodyType = RigidbodyType2D.Dynamic;
            chairRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Mayor impulso total
            float finalForce = chairThrowForce * 1.6f;
            chairRb.AddForce(launchDir * finalForce, ForceMode2D.Impulse);
        }

        if (audioSource != null && throwChairClip != null)
            audioSource.PlayOneShot(throwChairClip, throwChairVolume);
    }

    public void DoAreaDamage()
    {
        StartCoroutine(DelayedAreaDamage());
    }

    private IEnumerator DelayedAreaDamage()
    {
        // Esperamos a que el Rigidbody termine su movimiento antes de calcular el área
        yield return new WaitForFixedUpdate();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaDamageRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
                Rigidbody2D playerRb = hit.GetComponentInParent<Rigidbody2D>();

                if (playerHealth != null && playerRb != null)
                {
                    // Calculamos dirección de empuje y daño
                    Vector2 knockDir = (playerHealth.transform.position - transform.position).normalized;
                    int finalDamage = Mathf.RoundToInt(damage * 1.8f);
                    float pushForce = attackPushForce;

                    // Aplicamos daño y empuje
                    playerHealth.RegisterAttacker(gameObject);
                    playerHealth.TakeDamage(finalDamage);
                    StartCoroutine(ApplyKnockback(playerRb, knockDir, pushForce));
                }
            }
        }
    }


    public void OnAttackEnd()
    {
        isAttacking = false;
        isDashing = false;
        lastAttackTime = Time.time;
        hasAttackedInRange = true;
    }

    public void StartAttackHitbox() => canDealDamage = true;
    public void EndAttackHitbox() => canDealDamage = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DoDamage(collision);

        if (collision.CompareTag("Player") && !isDead)
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            if (playerHealth != null && playerRb != null)
            {
                Vector2 knockDir = (playerHealth.transform.position - transform.position).normalized;
                int finalDamage = damage;
                float pushForce = normalPushForce;

                if (canDealDamage || isAttacking)
                {
                    finalDamage = Mathf.RoundToInt(damage * 1.5f);
                    pushForce = attackPushForce;
                }

                playerHealth.RegisterAttacker(gameObject);
                playerHealth.TakeDamage(finalDamage);
                StartCoroutine(ApplyKnockback(playerRb, knockDir, pushForce));
            }
        }
    }

    private IEnumerator ApplyKnockback(Rigidbody2D playerRb, Vector2 direction, float force)
    {
        PlayerMovement move = playerRb.GetComponent<PlayerMovement>();
        if (move != null) move.IsKnockedBack = true;

        playerRb.linearVelocity = Vector2.zero;
        Vector2 knockVector = (direction.normalized + Vector2.up * 0.2f).normalized * force;
        playerRb.AddForce(knockVector, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        playerRb.linearVelocity = Vector2.zero;
        if (move != null) move.IsKnockedBack = false;
    }

    private void DoDamage(Collider2D collision)
    {
        if (collision.transform.root == transform) return;

        if (collision.CompareTag("Bullet"))
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("HolyBullet"))
        {
            TakeDamage(5);
            Destroy(collision.gameObject);
        }
    }


    private void TakeDamage(int amount)
    {
        if (!isInvulnerable && !isDead)
        {
            audioSource.PlayOneShot(GethitSound());
            currentHp -= amount;

            if (bossHealthSlider != null)
                bossHealthSlider.value = currentHp;

            if (hitEffect != null)
            {
                hitEffect.gameObject.SetActive(true);
                hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                hitEffect.Play();
            }

            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(playerTransform.position, transform.position);

                if (distanceToPlayer <= closeAttackRange + 0.5f)
                {
                    hitsWhileClose++;
                    lastHitTime = Time.time;

                    if (hitsWhileClose >= hitsToTriggerDash && !isAttacking && !isDashing && !isDead)
                    {
                        hitsWhileClose = 0;
                        StopAllCoroutines();
                        StartCoroutine(DoCounterDashAttack());
                        return;
                    }
                }
            }

            if (currentHp <= 0)
            {
                TriggerInstantDefeat();
                StartCoroutine(Death());
            }

            else { StartCoroutine(DamageFlashAndInvulnerability()); }               
        }
    }

    private IEnumerator DamageFlashAndInvulnerability()
    {
        isInvulnerable = true;
        spriteRenderer.color = Color.red;
        animator.SetTrigger("GetHit");

        yield return new WaitForSeconds(0.1f);

        float elapsed = 0f;
        while (elapsed < invulnerabilityTime)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashInterval / 2f);
            spriteRenderer.color = Color.clear;
            yield return new WaitForSeconds(flashInterval / 2f);
            elapsed += flashInterval;
        }

        spriteRenderer.color = Color.white;
        isInvulnerable = false;
    }

    private IEnumerator Death()
    {
        isDead = true;
        animator.SetTrigger("Death");

        if (transform.childCount > 0)
            transform.GetChild(0).gameObject.SetActive(false);

        if (hitEffect != null)
        {
            hitEffect.gameObject.SetActive(true);
            hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            hitEffect.Play();
        }

        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (hitEffect != null)
            yield return new WaitForSeconds(hitEffect.main.duration);

        // 🔹 Iniciar el desvanecimiento visual
        yield return StartCoroutine(FadeOutSprite(1.2f));

        // 🔹 Llamar a la secuencia final del boss
        TriggerDefeatSequence();

        // 🔹 Finalmente desactivamos el objeto
        gameObject.SetActive(false);

        // 🎥 Después de que el boss está completamente deshabilitado, enfocar al jugador
        var defeatSequence = FindAnyObjectByType<BossDefeatSequence>();
        if (defeatSequence != null)
        {
            defeatSequence.FocusOnPlayerAfterBossDefeated();
        }

    }
    private IEnumerator FadeOutSprite(float duration)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            yield break;

        Color startColor = sr.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        sr.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    private AudioClip GethitSound()
    {
        int random = Random.Range(0, 3);
        return random switch
        {
            0 => hit1,
            1 => hit2,
            2 => hit3,
            _ => null
        };
    }

    public void Celebrate()
    {
        if (isDead || celebrating) return;

        celebrating = true;
        isAttacking = false;
        isDashing = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        animator.SetBool("isMoving", false);
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Celebration");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, closeAttackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, rangeAttackDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaAttackDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chairAttackDistance);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, areaDamageRadius);
    }

    public void PlayAnimationEventSound()
    {
        if (audioSource != null && animationEventClip != null)
            audioSource.PlayOneShot(animationEventClip, animationEventVolume);
    }

    private void TriggerDefeatSequence()
    {
        var defeatSequence = FindAnyObjectByType<BossDefeatSequence>();
        if (defeatSequence != null)
        {
            defeatSequence.StartDefeatSequence();
        }
    }

    private void TriggerInstantDefeat()
    {
        // Evitar ejecución múltiple
        if (isDead) return;

        isDead = true;

        // 🔹 Forzar al Player a estado Idle y cortar control
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var move = player.GetComponent<PlayerMovement>();
            var gun = player.GetComponent<_GunController>();
            var input = player.GetComponent<PlayerInput>();
            var audio = player.GetComponent<AudioSource>();

            if (move) move.enabled = false;
            if (gun) gun.enabled = false;
            if (input) input.enabled = false;
            if (audio) audio.Stop();

            // Forzar Idle instantáneo
            var playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.ResetTrigger("isRunning");
                playerAnimator.SetBool("isMoving", false);
                playerAnimator.Play("Idle", 0, 0f);
            }

            // Detener Rigidbody
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        // 🔹 Llamar al sistema de cámara y victoria de inmediato
        var defeatSequence = FindAnyObjectByType<BossDefeatSequence>();
        if (defeatSequence != null)
        {
            defeatSequence.StartDefeatSequence();
        }

        // 🔹 Cortar todos los sonidos del Pollo Loco salvo la música
        if (audioSource != null)
            audioSource.Stop();
    }
    public void PlayPlumaImpact()
    {
        if (plumaImpactEffect == null) return;

        // 🔹 Reinicia y reproduce
        plumaImpactEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        plumaImpactEffect.Clear(true);
        plumaImpactEffect.Play(true);
    }
}
