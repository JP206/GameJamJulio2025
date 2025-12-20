using System.Collections;
using UnityEngine;

public class FinalBossCinematic : MonoBehaviour
{
    public static bool IsPlaying { get; private set; } = false;

    /// <summary>
    /// Resetea el flag de cinemática (necesario para Retry en Final Boss)
    /// </summary>
    public static void ResetCinematicState()
    {
        IsPlaying = false;
    }

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
        // 🔹 Esperar un frame para que BossSceneInitializer configure el player primero
        yield return null;
        
        // Buscar al jugador
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) 
        {
            Debug.LogError("❌ FinalBossCinematic: No se encontró el Player!");
            yield break;
        }

        // 🔹 Si es un RETRY, saltar la cinemática e ir directo a la pelea
        if (BossSceneInitializer.isRetry)
        {
            Debug.Log("🔄 Retry detectado - Saltando cinemática, iniciando pelea directa");
            yield return StartCoroutine(SetupRetryMode());
            yield break;
        }

        // === CINEMÁTICA NORMAL (primera vez) ===
        IsPlaying = true;

        // Bloquear mouse
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Desactivar rotación del mouse
        playerAim = player.GetComponentInChildren<PlayerAim>(true);
        if (playerAim != null)
            playerAim.enabled = false;

        // INMEDIATAMENTE bloquear al jugador para que no se mueva durante la espera
        playerController.DisablePlayer(player);

        // Esperar en el punto de spawn (sin movimiento)
        yield return new WaitForSeconds(1f);

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

        IsPlaying = false;
    }

    /// <summary>
    /// Configura la escena para Retry: sin cinemática, directo a la pelea
    /// </summary>
    private IEnumerator SetupRetryMode()
    {
        IsPlaying = false; // No hay cinemática
        
        // 🔹 CRÍTICO: Forzar TODAS las cinemáticas a FALSE
        UICanvasManager.ForceDisableAllCinematics();

        // 🔹 Desactivar objetos del portal inmediatamente
        if (triangleObject) triangleObject.SetActive(false);
        if (squareObject) squareObject.SetActive(false);
        if (portalObject) portalObject.SetActive(false);
        if (bossDoor) bossDoor.SetActive(false);

        // 🔹 Activar el boss y configurarlo para pelea
        if (bossIntro != null)
        {
            // Obtener referencia al boss desde bossIntro usando reflection
            var bossObjectField = bossIntro.GetType().GetField("bossObject", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (bossObjectField != null)
            {
                GameObject bossObject = bossObjectField.GetValue(bossIntro) as GameObject;
                if (bossObject != null)
                {
                    bossObject.SetActive(true);
                    
                    PolloLocoController bossController = bossObject.GetComponent<PolloLocoController>();
                    Animator bossAnim = bossObject.GetComponent<Animator>();
                    
                    if (bossController != null)
                    {
                        bossController.isCinematicMode = false; // Listo para pelear
                    }
                    
                    if (bossAnim != null)
                    {
                        bossAnim.Play("PolloLocoMovement", 0, 0f);
                    }
                }
            }
        }

        // 🔹 Configurar UI del boss (barra de HP llena)
        if (uiManager != null)
        {
            // Activar canvas de UI
            var canvasField = uiManager.GetType().GetField("uiCanvas", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (canvasField != null)
            {
                Canvas uiCanvas = canvasField.GetValue(uiManager) as Canvas;
                if (uiCanvas != null)
                    uiCanvas.gameObject.SetActive(true);
            }
            
            // Llenar barra de HP inmediatamente
            var sliderField = uiManager.GetType().GetField("enemyHpBar", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sliderField != null)
            {
                UnityEngine.UI.Slider hpBar = sliderField.GetValue(uiManager) as UnityEngine.UI.Slider;
                if (hpBar != null)
                {
                    hpBar.value = hpBar.maxValue;
                }
            }
        }

        // 🔹 Habilitar UICanvasManager (necesario para pausa y menús)
        uiManagerInstance = FindFirstObjectByType<UICanvasManager>();
        if (uiManagerInstance != null)
            uiManagerInstance.enabled = true;

        // 🔹 Habilitar controles del player COMPLETAMENTE usando método estático
        UICanvasManager.ForceEnablePlayerControls();

        // 🔹 Iniciar música del boss
        if (bossMusic != null)
        {
            bossMusic.volume = 1f;
            bossMusic.Play();
        }

        // 🔹 Esperar un frame y resetear el flag de retry
        yield return null;
        BossSceneInitializer.isRetry = false;
        
        Debug.Log("✅ Modo Retry configurado - Pelea iniciada");
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
