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

        var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        if (players.Length > 1)
        {
            foreach (var p in players)
            {
                if (p.gameObject.scene.name != "DontDestroyOnLoad")
                    Destroy(p.gameObject);
            }
        }

        var mainPlayer = FindAnyObjectByType<PlayerMovement>();
        if (mainPlayer == null)
        {
            yield break;
        }

        if (spawnPoint != null)
            mainPlayer.transform.position = spawnPoint.position;

        var sprite = mainPlayer.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.flipX = !faceRightOnSpawn;
            sprite.sortingOrder = 4;
        }

        var health = mainPlayer.GetComponent<PlayerHealth>();
        var gun = mainPlayer.GetComponent<_GunController>();
        if (GameManager.Instance != null)
            StartCoroutine(ApplyPlayerDataDelayed(health, gun));

        var cineCam = FindAnyObjectByType<CinemachineCamera>();
        if (cineCam != null)
            cineCam.Follow = mainPlayer.transform;

        var uiCam = FindAnyObjectByType<UICamera>();
        if (uiCam != null)
        {
            var playerField = typeof(UICamera).GetField("player",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField.SetValue(uiCam, mainPlayer.transform);
        }

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

        var trail = mainPlayer.GetComponent<TrailRenderer>();
        if (trail != null)
            trail.sortingOrder = 1;
    }

    private IEnumerator ApplyPlayerDataDelayed(PlayerHealth health, _GunController gun)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadPlayerData(health, gun);

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
                var fieldText = gun.GetType().GetField("ammoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var fieldShade = gun.GetType().GetField("ammoTextShade", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                fieldText?.SetValue(gun, ammoText);
                fieldShade?.SetValue(gun, ammoShade);

                gun.RefreshAmmoUI();

                assigned = true;
            }

            yield return new WaitForSeconds(0.2f);
        }

        var slider = FindAnyObjectByType<UnityEngine.UI.Slider>();
        if (slider != null && health != null)
        {
            slider.maxValue = health.MaxHealth;
            slider.value = health.CurrentHealth;
        }
    }
}
