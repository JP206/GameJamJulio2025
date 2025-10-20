using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Data")]
    public int playerScore;
    public float playerAmmo;
    public int playerHealth;

    [Header("Holy Meter Data")]
    public float holyCharge; // Solo guardamos el progreso visual (0–1)

    private void Awake()
    {
        // Singleton + persistencia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // --- Sincronizar ammo y vida constantemente ---
        var gun = FindAnyObjectByType<_GunController>();
        if (gun != null)
            playerAmmo = gun.GetAmmo();

        var health = FindAnyObjectByType<PlayerHealth>();
        if (health != null)
            playerHealth = health.CurrentHealth;

        // --- Sincronizar Holy Meter (solo progreso) ---
        var holy = FindAnyObjectByType<HolyMeterController>();
        if (holy != null)
            holyCharge = holy.GetCurrentCharge();
    }

    // Guarda el estado actual del jugador
    public void SavePlayerData(PlayerHealth health, _GunController gun, HolyMeterController holy)
    {
        if (health != null)
            playerHealth = health.CurrentHealth;

        if (gun != null)
            playerAmmo = gun.GetAmmo();

        if (holy != null)
            holyCharge = holy.GetCurrentCharge();
    }

    // Carga el estado guardado al entrar a una nueva escena
    public void LoadPlayerData(PlayerHealth health, _GunController gun, HolyMeterController holy)
    {
        if (health != null)
        {
            health.SetCurrentHealth(playerHealth);
            health.OnHealthChanged.Invoke(health.CurrentHealth, health.MaxHealth);
        }

        if (gun != null)
        {
            gun.SetAmmo(playerAmmo);
            gun.RefreshAmmoUI();
        }

        if (holy != null)
        {
            holy.SetCurrentCharge(holyCharge);
        }
    }

    // Si necesitás guardar puntuación global
    public void AddScore(int amount)
    {
        playerScore += amount;
    }

    public void ResetGameData()
    {
        playerScore = 0;
        playerAmmo = 0;
        playerHealth = 0;
        holyCharge = 0;
    }
}
