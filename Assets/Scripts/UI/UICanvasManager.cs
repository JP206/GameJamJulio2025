using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class UICanvasManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject gameOverCanvas;

    [Header("Game Over Fade")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float targetAlpha = 0.47f; // Alpha = 120/255

    private bool isPaused = false;

    public static bool IsGamePausedOrOver { get; private set; }
    
    /// <summary>
    /// Fuerza la habilitación de todos los controles del player (para Retry)
    /// </summary>
    public static void ForceEnablePlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) 
        {
            Debug.LogWarning("⚠️ ForceEnablePlayerControls: No se encontró el Player");
            return;
        }
        
        // Reactivar el GameObject
        player.SetActive(true);
        
        // Habilitar todos los componentes
        var move = player.GetComponent<PlayerMovement>();
        var gun = player.GetComponent<_GunController>();
        var input = player.GetComponent<PlayerInput>();
        var health = player.GetComponent<PlayerHealth>();
        var rb = player.GetComponent<Rigidbody2D>();
        var sprite = player.GetComponent<SpriteRenderer>();
        var animator = player.GetComponent<Animator>();
        var aim = player.GetComponentInChildren<PlayerAim>(true);
        
        if (move != null) 
        {
            move.enabled = true;
            move.ResetPlayerState();
        }
        if (gun != null) 
        {
            gun.enabled = true;
            gun.ResetHolyShotState();
        }
        if (input != null) input.enabled = true;
        if (health != null) health.ResetHealthState();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
        if (sprite != null)
        {
            sprite.enabled = true;
            sprite.color = Color.white;
        }
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isRunningBackwards", false);
            animator.Play("Player_Idle", 0, 0f);
        }
        if (aim != null) aim.enabled = true;
        
        // Restaurar cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        Debug.Log("✅ ForceEnablePlayerControls: Controles habilitados");
    }
    
    /// <summary>
    /// Fuerza TODOS los flags de cinemáticas a FALSE (para Retry en Final Boss)
    /// </summary>
    public static void ForceDisableAllCinematics()
    {
        // Usar reflection para forzar IntroCinematicManager.IsCinematicPlaying = false
        var introField = typeof(IntroCinematicManager).GetProperty("IsCinematicPlaying", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (introField != null && introField.CanWrite)
        {
            introField.SetValue(null, false);
        }
        else
        {
            // Si no tiene setter, usar el backing field directamente
            var backingField = typeof(IntroCinematicManager).GetField("<IsCinematicPlaying>k__BackingField", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            backingField?.SetValue(null, false);
        }
        
        // FinalBossCinematic ya tiene ResetCinematicState que lo pone en false
        FinalBossCinematic.ResetCinematicState();
        
        // FinalDoorCinematic también
        var doorField = typeof(FinalDoorCinematic).GetField("<IsPlaying>k__BackingField", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        doorField?.SetValue(null, false);
        
        // Resetear el flag de pausa también
        IsGamePausedOrOver = false;
        
        Debug.Log("✅ ForceDisableAllCinematics: Todas las cinemáticas desactivadas");
    }

    void Update()
    {
        // No permitir pausa durante cinemáticas o game over
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOverCanvas.activeSelf && !IsAnyCinematicPlaying())
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Verifica si alguna cinemática está reproduciéndose
    /// </summary>
    private bool IsAnyCinematicPlaying()
    {
        return IntroCinematicManager.IsCinematicPlaying 
            || FinalDoorCinematic.IsPlaying 
            || FinalBossCinematic.IsPlaying;
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseCanvas.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        IsGamePausedOrOver = isPaused;
    }

    public void ResumeGame()
    {
        TogglePause();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsGamePausedOrOver = false;
        
        string currentScene = SceneManager.GetActiveScene().name;
        
        // 🔹 Manejar según la escena
        if (currentScene == "Final Boss")
        {
            // 🔹 IMPORTANTE: En Final Boss NO resetear IntroCinematicManager (lo dejaría en true)
            // En cambio, forzar TODOS los flags de cinemática a FALSE
            FinalBossCinematic.ResetCinematicState();
            
            // 🔹 Forzar IntroCinematicManager a FALSE (no hay intro en Final Boss)
            ForceDisableAllCinematics();
            
            // En Final Boss: restaurar datos originales y DESTRUIR el player
            BossSceneInitializer.RestoreOriginalData();
            BossSceneInitializer.isRetry = true;
            Debug.Log("🔄 Retry en Final Boss - Cinemáticas desactivadas, saltando intro");
            
            // Destruir players para que BossSceneInitializer cree uno nuevo
            PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var player in allPlayers)
            {
                Destroy(player.gameObject);
            }
        }
        else
        {
            // En otras escenas: resetear cinemáticas para que funcionen al recargar
            IntroCinematicManager.ResetCinematicState();
            FinalBossCinematic.ResetCinematicState();
            
            // Destruir players y resetear GameManager
            PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var player in allPlayers)
            {
                Destroy(player.gameObject);
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGameData();
            }
            BossSceneInitializer.ResetOriginalData();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        IsGamePausedOrOver = false;
        
        // Resetear flags estáticos de cinemáticas
        IntroCinematicManager.ResetCinematicState();
        FinalBossCinematic.ResetCinematicState();
        
        // Destruir TODOS los players (activos e inactivos)
        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var player in allPlayers)
        {
            Destroy(player.gameObject);
        }
        
        // Resetear GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGameData();
        }
        
        // Resetear datos originales del boss scene
        BossSceneInitializer.ResetOriginalData();
        
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerGameOver()
    {
        // Bloquear pausa inmediatamente al morir
        IsGamePausedOrOver = true;
        StartCoroutine(DelayedGameOver());
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(1f);

        gameOverCanvas.SetActive(true);

        if (fadePanel != null)
        {
            Color color = fadePanel.color;
            float startAlpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                fadePanel.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            fadePanel.color = new Color(color.r, color.g, color.b, targetAlpha);
        }
    }
}
