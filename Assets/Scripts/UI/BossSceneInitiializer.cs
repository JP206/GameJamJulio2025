using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.UI;

public class BossSceneInitializer : MonoBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool faceRightOnSpawn = true;
    
    [Header("Player Prefab (para Retry)")]
    [SerializeField] private GameObject playerPrefab;

    private PlayerMovement mainPlayer;
    private PlayerHealth playerHealth;
    private _GunController gunController;

    // 🔹 Datos originales del jugador al entrar a la escena (para Retry)
    public static int originalPlayerHealth;
    public static float originalPlayerAmmo;
    public static float originalHolyCharge;
    public static bool hasOriginalData = false;
    
    // 🔹 Flag para indicar si es un Retry (saltar cinemática)
    public static bool isRetry = false;

    /// <summary>
    /// Restaura los datos originales al GameManager (llamado antes de Retry)
    /// </summary>
    public static void RestoreOriginalData()
    {
        if (hasOriginalData && GameManager.Instance != null)
        {
            GameManager.Instance.playerHealth = originalPlayerHealth;
            GameManager.Instance.playerAmmo = originalPlayerAmmo;
            GameManager.Instance.holyCharge = originalHolyCharge;
        }
    }

    /// <summary>
    /// Resetea los datos originales (llamado al salir de la escena)
    /// </summary>
    public static void ResetOriginalData()
    {
        hasOriginalData = false;
        isRetry = false;
        originalPlayerHealth = 0;
        originalPlayerAmmo = 0;
        originalHolyCharge = 0;
    }

    private IEnumerator Start()
    {
        yield return null;

        SetupPlayerInstance();
        SetupPlayerPosition();
        SetupCameraFollow();
        SetupUICamera();
        SetupPlayerHealthUI();
        SetupTrailRenderer();

        yield return StartCoroutine(LoadPlayerDataAndUI());
        
        // 🔹 Si es un Retry, forzar habilitación de controles después de todo
        if (isRetry)
        {
            yield return new WaitForSeconds(0.5f); // Esperar a que FinalBossCinematic termine su setup
            
            // Forzar todas las cinemáticas a FALSE
            UICanvasManager.ForceDisableAllCinematics();
            
            // Forzar controles del player
            UICanvasManager.ForceEnablePlayerControls();
            
            Debug.Log("🔄 BossSceneInitializer: Cinemáticas desactivadas y controles forzados para Retry");
        }
    }

    private void SetupPlayerInstance()
    {
        var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        if (players.Length > 1)
        {
            foreach (var p in players)
            {
                if (p.gameObject.scene.name != "DontDestroyOnLoad")
                    Destroy(p.gameObject);
            }
        }

        mainPlayer = FindAnyObjectByType<PlayerMovement>();
        
        // 🔹 Si no hay Player (caso Retry), instanciar uno nuevo desde el prefab
        if (mainPlayer == null && playerPrefab != null)
        {
            Debug.Log("🔄 No hay Player - Instanciando desde prefab para Retry");
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            DontDestroyOnLoad(newPlayer);
            mainPlayer = newPlayer.GetComponent<PlayerMovement>();
        }
        
        if (mainPlayer == null)
        {
            Debug.LogError("❌ No se pudo encontrar ni crear un Player!");
            return;
        }

        playerHealth = mainPlayer.GetComponent<PlayerHealth>();
        gunController = mainPlayer.GetComponent<_GunController>();
        
        // 🔹 Resetear el estado del player (por si viene de un Game Over)
        ResetPlayerState();
    }
    
    private void ResetPlayerState()
    {
        if (mainPlayer == null) return;
        
        // Reactivar el GameObject si estaba desactivado
        mainPlayer.gameObject.SetActive(true);
        
        // Resetear PlayerMovement
        mainPlayer.ResetPlayerState();
        mainPlayer.enabled = true;
        
        // Resetear PlayerHealth
        if (playerHealth != null)
        {
            playerHealth.ResetHealthState();
        }
        
        // Resetear GunController
        if (gunController != null)
        {
            gunController.enabled = true;
            gunController.ResetHolyShotState();
        }
        
        // Resetear PlayerInput
        var playerInput = mainPlayer.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null)
            playerInput.enabled = true;
        
        // Resetear Rigidbody
        var rb = mainPlayer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
        }
        
        // Resetear SpriteRenderer
        var sprite = mainPlayer.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = Color.white;
            sprite.enabled = true;
        }
        
        // Resetear Animator
        var animator = mainPlayer.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isRunningBackwards", false);
            animator.Play("Player_Idle", 0, 0f);
        }
        
        // Resetear PlayerAim
        var playerAim = mainPlayer.GetComponentInChildren<PlayerAim>(true);
        if (playerAim != null)
            playerAim.enabled = true;
        
        Debug.Log("✅ Estado del Player reseteado completamente");
    }

    private void SetupPlayerPosition()
    {
        if (mainPlayer == null) return;

        if (spawnPoint != null)
            mainPlayer.transform.position = spawnPoint.position;

        var sprite = mainPlayer.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.flipX = !faceRightOnSpawn;
            sprite.sortingOrder = 4;
        }
    }

    private void SetupCameraFollow()
    {
        var cineCam = FindAnyObjectByType<CinemachineCamera>();
        if (cineCam != null && mainPlayer != null)
            cineCam.Follow = mainPlayer.transform;
    }

    private void SetupUICamera()
    {
        var uiCam = FindAnyObjectByType<UICamera>();
        if (uiCam != null && mainPlayer != null)
        {
            var playerField = typeof(UICamera).GetField("player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField?.SetValue(uiCam, mainPlayer.transform);
        }
    }

    private void SetupPlayerHealthUI()
    {
        if (playerHealth == null) return;

        var sliders = FindObjectsByType<Slider>(FindObjectsSortMode.None);
        foreach (var slider in sliders)
        {
            if (slider.name.ToLower().Contains("hp") || slider.name.ToLower().Contains("health"))
            {
                playerHealth.OnHealthChanged.RemoveAllListeners();

                playerHealth.OnHealthChanged.AddListener((current, max) =>
                {
                    slider.maxValue = max;
                    slider.value = current;
                });

                slider.maxValue = playerHealth.MaxHealth;
                slider.value = playerHealth.CurrentHealth;
                break;
            }
        }
    }

    private void SetupTrailRenderer()
    {
        if (mainPlayer == null) return;

        var trail = mainPlayer.GetComponent<TrailRenderer>();
        if (trail != null)
            trail.sortingOrder = 1;
    }

    // ============================================================
    // 🔹 Carga de datos persistentes del GameManager
    // ============================================================
    private IEnumerator LoadPlayerDataAndUI()
    {
        // Espera un frame para asegurar que todo esté cargado
        yield return null;

        if (GameManager.Instance != null)
        {
            // 🔹 Guardar los datos originales la PRIMERA vez (para Retry)
            if (!hasOriginalData)
            {
                originalPlayerHealth = GameManager.Instance.playerHealth;
                originalPlayerAmmo = GameManager.Instance.playerAmmo;
                originalHolyCharge = GameManager.Instance.holyCharge;
                hasOriginalData = true;
                Debug.Log($"💾 Datos originales guardados: HP={originalPlayerHealth}, Ammo={originalPlayerAmmo}");
            }

            var holyMeter = FindAnyObjectByType<HolyMeterController>();
            GameManager.Instance.LoadPlayerData(playerHealth, gunController, holyMeter);
        }

        yield return StartCoroutine(WaitForHUDAndAssignAmmo());
    }

    // ============================================================
    // 🔹 Vincula la UI de munición (TextMeshPro) al GunController
    // ============================================================
    private IEnumerator WaitForHUDAndAssignAmmo()
    {
        if (gunController == null) yield break;

        bool assigned = false;

        while (!assigned)
        {
            var uiTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
            TMPro.TextMeshProUGUI ammoText = null;
            TMPro.TextMeshProUGUI ammoShade = null;

            foreach (var text in uiTexts)
            {
                string name = text.name.ToLower();
                if (name.Contains("ammo text") && !name.Contains("shade"))
                    ammoText = text;
                if (name.Contains("ammo text shade"))
                    ammoShade = text;
            }

            if (ammoText != null && ammoShade != null)
            {
                var fieldText = gunController.GetType().GetField("ammoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var fieldShade = gunController.GetType().GetField("ammoTextShade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                fieldText?.SetValue(gunController, ammoText);
                fieldShade?.SetValue(gunController, ammoShade);

                gunController.RefreshAmmoUI();
                assigned = true;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }


}
