using UnityEngine;

public class UICamera : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float xOffsetRight;
    [SerializeField] float xOffsetLeft;
    [SerializeField] float yOffset;
    [SerializeField] float zPosition = -10f;
    [SerializeField] float smoothTime = 0.15f;

    private bool isFacingRight = true;
    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        transform.position = GetTargetPosition();
    }

    void LateUpdate()
    {
        CheckPlayerDirection();

        Vector3 targetPosition = GetTargetPosition();
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    void CheckPlayerDirection()
    {
        if (player.localScale.x > 0)
        {
            isFacingRight = true;
        }
        else if (player.localScale.x < 0)
        {
            isFacingRight = false;
        }
    }

    Vector3 GetTargetPosition()
    {
        float xOffset = isFacingRight ? xOffsetRight : xOffsetLeft;

        return new Vector3(
            player.position.x + xOffset,
            player.position.y + yOffset,
            zPosition
        );
    }
}
