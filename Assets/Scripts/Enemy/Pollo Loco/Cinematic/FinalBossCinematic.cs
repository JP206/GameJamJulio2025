using System.Collections;
using UnityEngine;

public class FinalBossCinematic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinematicPlayerController playerController;
    [SerializeField] private CinematicBarsController barsController;
    [SerializeField] private CinematicCameraFocus cameraFocus;
    [SerializeField] private CinematicBossIntro bossIntro;
    [SerializeField] private CinematicUIManager uiManager;

    [Header("Objects")]
    [SerializeField] private GameObject triangleObject;
    [SerializeField] private GameObject squareObject;
    [SerializeField] private GameObject portalObject;
    [SerializeField] private GameObject bossDoor;

    [Header("Audio")]
    [SerializeField] private AudioSource bossMusic;
    [SerializeField] private float musicFadeInTime = 2f;

    [Header("Timings")]
    [SerializeField] private float delayBeforeWalk = 1f;
    [SerializeField] private float delayTriangle = 0.5f;
    [SerializeField] private float delaySquare = 0.5f;
    [SerializeField] private float delayPortal = 1f;
    [SerializeField] private float delayDoor = 1.5f;
    [SerializeField] private float delayBeforeBossActive = 1f;

    private GameObject player;
    private PlayerAim playerAim;
    private UICanvasManager uiManagerInstance;

    private IEnumerator Start()
    {
        // Bloquear mouse
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Buscar al jugador
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        // Desactivar rotación del mouse
        playerAim = player.GetComponentInChildren<PlayerAim>(true);
        if (playerAim != null)
            playerAim.enabled = false;

        yield return new WaitForSeconds(1f);

        // Desactivar control del jugador
        playerController.DisablePlayer(player);

        // Desactivar UI Manager global (evita que se abra el menú de pausa)
        uiManagerInstance = FindFirstObjectByType<UICanvasManager>();
        if (uiManagerInstance != null)
            uiManagerInstance.enabled = false;

        // Mostrar barras cinematográficas
        if (barsController != null)
            yield return StartCoroutine(barsController.ShowBars());

        yield return new WaitForSeconds(delayBeforeWalk);

        // Movimiento automático del jugador
        yield return StartCoroutine(playerController.PlayerWalk(player));

        // Secuencia de cierre (símbolos, portal, puerta)
        yield return StartCoroutine(CloseSequence());

        yield return new WaitForSeconds(delayBeforeBossActive);

        // Intro del jefe
        if (bossIntro != null)
            yield return StartCoroutine(bossIntro.PlayBossIntro(cameraFocus, uiManager));

        // Ocultar barras
        if (barsController != null)
            yield return StartCoroutine(barsController.HideBars());

        // Reactivar control del jugador
        playerController.EnablePlayer(player);

        // Reactivar rotación del mouse
        if (playerAim != null)
            playerAim.enabled = true;

        // Reactivar UI Manager (permite pausa y menús nuevamente)
        if (uiManagerInstance != null)
            uiManagerInstance.enabled = true;

        // Reproducir música del boss con fade-in
        if (bossMusic != null)
            StartCoroutine(FadeInMusic(bossMusic, musicFadeInTime));

        // Restaurar cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // === Fading suave de música ===
    private IEnumerator FadeInMusic(AudioSource music, float duration)
    {
        music.volume = 0f;
        music.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            music.volume = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        music.volume = 1f;
    }

    private IEnumerator CloseSequence()
    {
        if (triangleObject)
        {
            Animator anim = triangleObject.GetComponent<Animator>();
            if (anim) anim.Play("TriangleDisappear", 0, 0f);
            yield return new WaitForSeconds(delayTriangle);
            triangleObject.SetActive(false);
        }

        if (squareObject)
        {
            Animator anim = squareObject.GetComponent<Animator>();
            if (anim) anim.Play("SquareDisappear", 0, 0f);
            yield return new WaitForSeconds(delaySquare);
            squareObject.SetActive(false);
        }

        if (portalObject)
        {
            Animator anim = portalObject.GetComponent<Animator>();
            if (anim) anim.Play("PortalClose", 0, 0f);
            yield return new WaitForSeconds(delayPortal);
            portalObject.SetActive(false);
        }

        if (bossDoor)
        {
            Animator anim = bossDoor.GetComponent<Animator>();
            if (anim) anim.Play("Close", 0, 0f);
            yield return new WaitForSeconds(delayDoor);
            bossDoor.SetActive(false);
        }
    }
}
