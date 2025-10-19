using UnityEngine;
using System.Collections;

public class PolloLocoAttackHandler : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D hitbox;
    private PolloLocoController controller;

    [Header("General Settings")]
    [SerializeField] private float attackPause;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float counterDashMultiplier;
    [SerializeField] private float counterDashRecovery;

    [Header("Area Attack Settings")]
    [SerializeField] private float areaJumpSpeed = 25f;
    [SerializeField] private float areaJumpDuration = 0.6f;
    [SerializeField] private float areaJumpHeightBoost = 6f;

    private bool isDashing = false;
    private bool canDealDamage = false;

    public bool IsDashing => isDashing;

    public void Initialize(Animator anim, Rigidbody2D body, Collider2D hit, PolloLocoController ctrl)
    {
        animator = anim;
        rb = body;
        hitbox = hit;
        controller = ctrl;
    }

    // === ATAQUE MELEE ===
    public IEnumerator MeleeAttack(System.Action onAttackEnd)
    {
        if (animator == null) yield break;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackPause);
        onAttackEnd?.Invoke();
    }

    // === DASH NORMAL ===
    public IEnumerator DashAttack(Vector2 direction, System.Action onAttackEnd)
    {
        if (rb == null || animator == null) yield break;
        isDashing = true;
        rb.linearVelocity = direction.normalized * dashSpeed;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackPause);
        onAttackEnd?.Invoke();
    }

    // === COUNTER DASH ===
    public IEnumerator CounterDash(Vector2 direction, System.Action onAttackEnd)
    {
        if (rb == null || animator == null) yield break;
        isDashing = true;
        rb.linearVelocity = direction.normalized * (dashSpeed * counterDashMultiplier);
        yield return new WaitForSeconds(dashDuration);
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackPause * counterDashRecovery);
        onAttackEnd?.Invoke();
    }

    public IEnumerator AreaAttack(Vector2 direction, System.Action onAttackEnd)
    {
        if (rb == null || animator == null)
            yield break;

        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("AreaAttack");

        Vector2 jumpVelocity = direction.normalized * areaJumpSpeed + Vector2.up * areaJumpHeightBoost;
        rb.AddForce(jumpVelocity, ForceMode2D.Impulse);

        yield return new WaitForSeconds(areaJumpDuration);

        rb.linearVelocity = Vector2.zero;

        onAttackEnd?.Invoke();
    }


    public void StartAttackHitbox() => ToggleHitbox(true);
    public void EndAttackHitbox() => ToggleHitbox(false);

    private void ToggleHitbox(bool state)
    {
        canDealDamage = state;
        if (hitbox != null)
            hitbox.enabled = state;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        controller?.SendMessage("DoDamage", other, SendMessageOptions.DontRequireReceiver);
    }
}
