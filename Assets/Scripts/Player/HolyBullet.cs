using UnityEngine;

public class HolyBullet : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifeTime = 2f;

    private Vector2 direction;

    private void OnEnable()
    {
        Invoke(nameof(Disable), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void Disable()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Building"))
        {
            gameObject.SetActive(false);
        }
    }
}
