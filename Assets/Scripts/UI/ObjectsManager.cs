using UnityEngine;
using System.Collections;

public class ObjectsManager : MonoBehaviour
{
    [SerializeField] private float respawnTime = 3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ammo"))
        {
            Debug.Log("Bucket tocado: " + other.name);
            StartCoroutine(DeactivateAndReactivate(other.gameObject));
        }
    }

    private IEnumerator DeactivateAndReactivate(GameObject obj)
    {
        obj.SetActive(false);
        yield return new WaitForSeconds(respawnTime);
        obj.SetActive(true);
    }
}
