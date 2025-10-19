using UnityEngine;

public class PolloLocoBoundaryFixer : MonoBehaviour
{
    [SerializeField] private Transform ringTransform;
    [SerializeField] private float offset = 0.3f;

    private Rigidbody2D rb;
    private Collider2D ringCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (ringTransform != null)
            ringCollider = ringTransform.GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        if (rb == null || ringTransform == null) return;

        Bounds totalBounds = new Bounds();
        bool initialized = false;

        foreach (var col in ringTransform.GetComponentsInChildren<BoxCollider2D>())
        {
            if (!initialized)
            {
                totalBounds = col.bounds;
                initialized = true;
            }
            else
            {
                totalBounds.Encapsulate(col.bounds);
            }
        }

        Vector2 pos = rb.position;

        float clampedX = Mathf.Clamp(pos.x, totalBounds.min.x + offset, totalBounds.max.x - offset);
        float clampedY = Mathf.Clamp(pos.y, totalBounds.min.y + offset, totalBounds.max.y - offset);

        if (pos.x != clampedX || pos.y != clampedY)
        {
            rb.position = new Vector2(clampedX, clampedY);
            rb.linearVelocity = Vector2.zero;
        }
    }
}
