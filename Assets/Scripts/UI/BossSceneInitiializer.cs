using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.UI;

public class BossSceneInitializer : MonoBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool faceRightOnSpawn = true;

    private PlayerMovement mainPlayer;
    private PlayerHealth playerHealth;
    private _GunController gunController;

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
        if (mainPlayer == null)
        {
            return;
        }

        playerHealth = mainPlayer.GetComponent<PlayerHealth>();
        gunController = mainPlayer.GetComponent<_GunController>();
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
        {
            cineCam.Follow = mainPlayer.transform;
        }
    }

    private void SetupUICamera()
    {
        var uiCam = FindAnyObjectByType<UICamera>();
        if (uiCam != null && mainPlayer != null)
        {
            var playerField = typeof(UICamera).GetField("player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField.SetValue(uiCam, mainPlayer.transform);
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

    private IEnumerator LoadPlayerDataAndUI()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadPlayerData(playerHealth, gunController);

        yield return StartCoroutine(WaitForHUDAndAssignAmmo());
    }

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
