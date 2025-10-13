using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.UI;

public class BossSceneInitializer : MonoBehaviour
{
    [Header("Player Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool faceRightOnSpawn = true;

    private IEnumerator Start()
    {
        yield return null;

        // --- Eliminar Player duplicado ---
        var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        if (players.Length > 1)
        {
            foreach (var p in players)
            {
                if (p.gameObject.scene.name != "DontDestroyOnLoad")
                    Destroy(p.gameObject);
            }
        }

        // --- Obtener Player persistente ---
        var mainPlayer = FindAnyObjectByType<PlayerMovement>();
        if (mainPlayer == null)
        {
            yield break;
        }

        // --- Reposicionar y orientar ---
        if (spawnPoint != null)
            mainPlayer.transform.position = spawnPoint.position;

        var sprite = mainPlayer.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.flipX = !faceRightOnSpawn;
            sprite.sortingOrder = 4;
        }

        // --- Restaurar datos ---
        var health = mainPlayer.GetComponent<PlayerHealth>();
        var gun = mainPlayer.GetComponent<_GunController>();
        if (GameManager.Instance != null)
            StartCoroutine(ApplyPlayerDataDelayed(health, gun));

        // --- Vincular Cinemachine ---
        var cineCam = FindAnyObjectByType<CinemachineCamera>();
        if (cineCam != null)
        {
            cineCam.Follow = mainPlayer.transform;
        }

        // --- Vincular UICamera ---
        var uiCam = FindAnyObjectByType<UICamera>();
        if (uiCam != null)
        {
            var playerField = typeof(UICamera).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField.SetValue(uiCam, mainPlayer.transform);
        }

        // --- Reconectar textos de Ammo ---
        var gunController = FindAnyObjectByType<_GunController>();
        if (gunController != null)
        {
            var uiTexts = FindObjectsByType<TMPro.TextMeshProUGUI>(FindObjectsSortMode.None);
            TMPro.TextMeshProUGUI ammoText = null;
            TMPro.TextMeshProUGUI ammoShade = null;

            foreach (var text in uiTexts)
            {
                if (text.name.ToLower().Contains("ammo text") && !text.name.ToLower().Contains("shade"))
                    ammoText = text;

                if (text.name.ToLower().Contains("ammo text shade"))
                    ammoShade = text;
            }

            if (ammoText != null)
            {
                gunController.GetType().GetField("ammoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(gunController, ammoText);
            }

            if (ammoShade != null)
            {
                gunController.GetType().GetField("ammoTextShade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(gunController, ammoShade);
            }

            var ammoValue = gunController.GetAmmo();
            if (ammoText != null)
                ammoText.text = "Ammo: " + ammoValue.ToString();
            if (ammoShade != null)
                ammoShade.text = ammoText.text;

        }

        // --- Reconectar barra de vida ---
        var playerHealth = mainPlayer.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
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

        // --- Ajustar el sorting layer del Trail Renderer del Player ---
        var trail = mainPlayer.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.sortingOrder = 1;
        }
    }

    private IEnumerator ApplyPlayerDataDelayed(PlayerHealth health, _GunController gun)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadPlayerData(health, gun);
        }

        var slider = FindAnyObjectByType<UnityEngine.UI.Slider>();
        if (slider != null && health != null)
        {
            slider.maxValue = health.MaxHealth;
            slider.value = health.CurrentHealth;
        }

        yield return null;

        slider = FindAnyObjectByType<UnityEngine.UI.Slider>();
        if (slider != null && health != null)
        {
            slider.maxValue = health.MaxHealth;
            slider.value = health.CurrentHealth;
        }
    }
}
