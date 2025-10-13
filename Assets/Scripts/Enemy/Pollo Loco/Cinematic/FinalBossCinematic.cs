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

    [Header("Timings")]
    [SerializeField] private float delayBeforeWalk = 1f;
    [SerializeField] private float delayTriangle = 0.5f;
    [SerializeField] private float delaySquare = 0.5f;
    [SerializeField] private float delayPortal = 1f;
    [SerializeField] private float delayDoor = 1.5f;
    [SerializeField] private float delayBeforeBossActive = 1f;

    private GameObject player;

    private IEnumerator Start()
    {
        // Buscar el Player persistente
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        // Esperar un momento para que BossSceneInitializer termine de configurar todo
        yield return new WaitForSeconds(1f);

        // 🔹 Desactivar control completo del jugador
        playerController.DisablePlayer(player);

        // 🔹 Mostrar barras cinematográficas
        if (barsController != null)
            yield return StartCoroutine(barsController.ShowBars());

        // 🔹 Esperar antes de caminar
        yield return new WaitForSeconds(delayBeforeWalk);

        // 🔹 Caminar automáticamente hasta el punto
        yield return StartCoroutine(playerController.PlayerWalk(player));

        // 🔹 Cerrar símbolos / portal / puerta
        yield return StartCoroutine(CloseSequence());

        // 🔹 Esperar antes del boss
        yield return new WaitForSeconds(delayBeforeBossActive);

        // 🔹 Activar boss + cámara + UI
        if (bossIntro != null)
            yield return StartCoroutine(bossIntro.PlayBossIntro(cameraFocus, uiManager));

        // 🔹 Ocultar barras negras
        if (barsController != null)
            yield return StartCoroutine(barsController.HideBars());

        // 🔹 Reactivar control del jugador al final
        playerController.EnablePlayer(player);
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
