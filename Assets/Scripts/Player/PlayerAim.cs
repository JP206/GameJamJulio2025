using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private SpriteRenderer playerSprite;
    private Vector3 initialFirePointLocalPos;

    void Start()
    {
        if (firePoint != null)
            initialFirePointLocalPos = firePoint.localPosition;
    }

    void Update()
    {
        AimAtMouse();
    }

    private void AimAtMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = mousePos - (Vector2)transform.position;

        bool facingLeft = mousePos.x < transform.position.x;

        if (playerSprite != null)
            playerSprite.flipX = facingLeft;

        if (firePoint != null)
        {
            firePoint.localPosition = new Vector3(
                facingLeft ? -initialFirePointLocalPos.x : initialFirePointLocalPos.x,
                initialFirePointLocalPos.y,
                initialFirePointLocalPos.z
            );
        }
    }
}
