using UnityEngine;
using System.Collections;

public class ObjectsManager : MonoBehaviour
{
    [SerializeField] private float bucketRespawnTime = 3f;
    [SerializeField] private float bottleRespawnTime = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ammo"))
        {
            StartCoroutine(DeactivateAndReactivate(other.gameObject));
        }

        if (other.CompareTag("Life"))
        {
            StartCoroutine(DeactivateAndReactivate(other.gameObject));
        }
    }

    private IEnumerator DeactivateAndReactivate(GameObject obj)
    {
        obj.SetActive(false);
        if (obj.CompareTag("Ammo"))
        {
            yield return new WaitForSeconds(bucketRespawnTime);
            obj.SetActive(true);
        }
        else if (obj.CompareTag("Life"))
        {
            yield return new WaitForSeconds(bottleRespawnTime);
            obj.SetActive(true);
        }
    }
}
