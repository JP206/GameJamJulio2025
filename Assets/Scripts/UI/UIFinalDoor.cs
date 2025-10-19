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
    [SerializeField] private GameObject doorExternalColliders;

    [Header("Particles")]
    [SerializeField] private GameObject particleSystemA;
    [SerializeField] private GameObject particleSystemB;

    [Header("Timings")]
    [SerializeField] private float delayPortal = 1.5f;
    [SerializeField] private float delaySymbols = 1.5f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float triangleDelay = 0.5f;
    [SerializeField] private float delayBeforeSceneLoad = 2.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip emergeClip;
    [SerializeField] private AudioClip portalClip;
    [SerializeField] private AudioClip symbolsClip;

    private bool isActive = false;
    private BoxCollider2D doorCollider;
    private ParticleSystem psA;
    private ParticleSystem psB;

    private void Awake()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        if (doorCollider != null)
            doorCollider.enabled = false;

        if (doorExternalColliders != null)
            doorExternalColliders.SetActive(false);

        if (particleSystemA != null)
        {
            psA = particleSystemA.GetComponent<ParticleSystem>();
            particleSystemA.SetActive(false);
        }

        if (particleSystemB != null)
        {
            psB = particleSystemB.GetComponent<ParticleSystem>();
            particleSystemB.SetActive(false);
        }
    }

    public void ShowFinalDoor()
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        EnableDoorRenderer();

        if (doorAnimator != null)
            doorAnimator.Play("emerge", 0, 0f);

        if (particleSystemA != null)
        {
            particleSystemA.SetActive(true);
            psA.Play();
        }

        if (particleSystemB != null)
        {
            particleSystemB.SetActive(true);
            psB.Play();
        }

        yield return StartCoroutine(FadeInSprite(GetComponent<SpriteRenderer>(), 2f));

        if (doorCollider != null)
            doorCollider.enabled = true;

        if (doorExternalColliders != null)
            doorExternalColliders.SetActive(true);

        yield return new WaitForSeconds(delayPortal);
        ActivatePortal();

        yield return new WaitForSeconds(delaySymbols);
        yield return StartCoroutine(ActivateSymbols());

        yield return new WaitForSeconds(delayBeforeSceneLoad);
        SaveAndLoadBossScene();
    }

    public void StopParticles()
    {
        if (psA != null)
            psA.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (psB != null)
            psB.Stop(true, ParticleSystemStopBehavior.StopEmitting);
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

        PlayPortalSound();
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
        yield return new WaitForEndOfFrame();

        var playerHealth = FindAnyObjectByType<PlayerHealth>();
        var gun = FindAnyObjectByType<_GunController>();

        if (playerHealth != null && gun != null)
            GameManager.Instance.SavePlayerData(playerHealth, gun);

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene("FinalBoss");
    }

    public void PlayEmergeSound()
    {
        StopCurrentSound();
        if (audioSource != null && emergeClip != null)
            audioSource.PlayOneShot(emergeClip);
    }

    public void PlayPortalSound()
    {
        StartCoroutine(CrossfadeToClip(portalClip, 1.0f, 0.8f));
    }

    public void PlaySymbolsSound()
    {
        StartCoroutine(CrossfadeToClip(symbolsClip, 1.0f, 0.8f));
    }

    public void StopCurrentSound()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private IEnumerator CrossfadeToClip(AudioClip newClip, float fadeInDuration, float overlapDuration)
    {
        if (audioSource == null || newClip == null)
            yield break;

        float startVolume = audioSource.volume;

        // 🔊 Creamos un segundo AudioSource temporal para el crossfade
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = newClip;
        tempSource.volume = 0f;
        tempSource.playOnAwake = false;
        tempSource.loop = false;
        tempSource.spatialBlend = audioSource.spatialBlend;

        tempSource.Play();

        float time = 0f;
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeInDuration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            tempSource.volume = Mathf.Lerp(0f, startVolume, t);
            yield return null;
        }

        // 🚫 En lugar de destruir el source original, lo dejamos vivo y listo para el siguiente sonido
        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.volume = tempSource.volume;
        audioSource.Play();

        Destroy(tempSource); // solo destruimos el temporal
    }

}
