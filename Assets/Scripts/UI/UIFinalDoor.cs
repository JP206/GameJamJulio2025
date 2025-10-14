using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIFinalDoor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private GameObject portalObject;
    [SerializeField] private GameObject squareObject;
    [SerializeField] private GameObject triangleObject;

    [Header("External Colliders")]
    [SerializeField] private GameObject doorExternalColliders; // 🔹 hijo con el PolygonCollider2D

    [Header("Timings")]
    [SerializeField] private float delayPortal = 1.5f;
    [SerializeField] private float delaySymbols = 1.5f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float triangleDelay = 0.5f;
    [SerializeField] private float delayBeforeSceneLoad = 2.5f;

    private bool isActive = false;
    private BoxCollider2D doorCollider;

    private void Awake()
    {
        // Cache del collider y lo desactivamos al inicio
        doorCollider = GetComponent<BoxCollider2D>();
        if (doorCollider != null)
            doorCollider.enabled = false;

        // 🔹 aseguramos que el hijo con los colliders externos empiece desactivado
        if (doorExternalColliders != null)
            doorExternalColliders.SetActive(false);
    }

    public void ShowFinalDoor()
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        // --- Puerta ---
        EnableDoorRenderer();
        if (doorAnimator != null) doorAnimator.Play("emerge", 0, 0f);

        // Fade-in antes de habilitar el collider
        yield return StartCoroutine(FadeInSprite(GetComponent<SpriteRenderer>(), 2f));

        if (doorCollider != null)
            doorCollider.enabled = true;

        // 🔹 activar el collider del hijo cuando se activa el principal
        if (doorExternalColliders != null)
        {
            doorExternalColliders.SetActive(true);;
        }

        // --- Portal ---
        yield return new WaitForSeconds(delayPortal);
        ActivatePortal();

        // --- Símbolos ---
        yield return new WaitForSeconds(delaySymbols);
        yield return StartCoroutine(ActivateSymbols());

        // --- Transición a la escena del Boss ---
        yield return new WaitForSeconds(delayBeforeSceneLoad);
        SaveAndLoadBossScene();
    }

    private void EnableDoorRenderer()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            SetAlpha(sr, 0f);
        }
    }

    private void ActivatePortal()
    {
        if (portalObject != null)
            portalObject.SetActive(true);

        var portalSR = portalObject?.GetComponent<SpriteRenderer>();
        if (portalSR != null)
        {
            SetAlpha(portalSR, 0f);
            StartCoroutine(FadeInSprite(portalSR, fadeDuration));
        }

        if (portalAnimator != null)
            portalAnimator.Play("Portal", 0, 0f);
    }

    private IEnumerator ActivateSymbols()
    {
        if (squareObject != null)
        {
            squareObject.SetActive(true);
            var sr = squareObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                SetAlpha(sr, 0f);
                yield return StartCoroutine(FadeInSprite(sr, fadeDuration));
            }
        }

        yield return new WaitForSeconds(triangleDelay);

        if (triangleObject != null)
        {
            triangleObject.SetActive(true);
            var sr = triangleObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                SetAlpha(sr, 0f);
                StartCoroutine(FadeInSprite(sr, fadeDuration));
            }
        }
    }

    private IEnumerator FadeInSprite(SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color color = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / duration);
            sr.color = color;
            yield return null;
        }

        color.a = 1f;
        sr.color = color;
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }

    private void SaveAndLoadBossScene()
    {
        StartCoroutine(SaveAndLoadBossSceneRoutine());
    }

    private IEnumerator SaveAndLoadBossSceneRoutine()
    {
        yield return new WaitForEndOfFrame(); // Espera a que todo esté inicializado

        var playerHealth = FindAnyObjectByType<PlayerHealth>();
        var gun = FindAnyObjectByType<_GunController>();

        if (playerHealth != null && gun != null)
        {
            GameManager.Instance.SavePlayerData(playerHealth, gun);
        }

        // Espera mínima para que se procese el guardado
        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene("FinalBoss");
    }
}
