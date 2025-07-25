using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float speed;

    Transform playerTransform;

    void Start()
    {
        // logica para encontrar playerTransform
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, speed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ajustar el nombre del tag
        if (collision.CompareTag("Bullet"))
        {
            gameObject.SetActive(false);
        }

        // ajustar el nombre del tag
        if (collision.CompareTag("Player"))
        {
            // logica de sacarle vida
        }
    }
}
