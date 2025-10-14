using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CinematicPlayerController : MonoBehaviour
{
    [SerializeField] private Transform playerTargetPoint;
    [SerializeField] private float walkDuration = 1.5f;

    private bool isFrozen = false;

    public void DisablePlayer(GameObject player)
    {
        isFrozen = true;

        var move = player.GetComponent<PlayerMovement>();
        var gun = player.GetComponent<_GunController>();
        var input = player.GetComponent<PlayerInput>();

        if (move) move.enabled = false;
        if (gun) gun.enabled = false;
        if (input) input.enabled = false;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    public void EnablePlayer(GameObject player)
    {
        isFrozen = false;

        var move = player.GetComponent<PlayerMovement>();
        var gun = player.GetComponent<_GunController>();
        var input = player.GetComponent<PlayerInput>();

        if (move) move.enabled = true;
        if (gun) gun.enabled = true;
        if (input) input.enabled = true;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public IEnumerator PlayerWalk(GameObject player)
    {
        if (player == null || !playerTargetPoint)
            yield break;

        Vector3 start = player.transform.position;
        Vector3 end = playerTargetPoint.position;

        yield return new WaitForSeconds(0.15f);

        Animator anim = player.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("isRunning", true);

        float elapsed = 0f;
        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / walkDuration;
            player.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        if (anim != null)
            anim.SetBool("isRunning", false);
    }

    public bool IsPlayerFrozen()
    {
        return isFrozen;
    }
}
