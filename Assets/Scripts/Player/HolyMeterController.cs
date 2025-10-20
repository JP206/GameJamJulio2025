using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HolyMeterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HolyMeterUI holyMeterUI;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject overlayPanel;

    [Header("Settings")]
    [SerializeField, Tooltip("Cantidad que suma por golpe (0.01 = 1%)")]
    private float progressPerHit = 0.03f;
    [SerializeField] private float holyShotDuration = 3f;
    [SerializeField] private float bossHitCooldown = 2f;

    [Header("Visual FX")]
    [SerializeField] private float zoomFactor = 0.9f;
    [SerializeField] private float transitionTime = 0.2f;
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float pulseSpeed = 3f;

    private bool canGainEnergy = true;
    private bool isCharged = false;
    private bool isActive = false;

    private float originalCameraSize;
    private Image meterImage;
    private Color originalColor;

    private void Start()
    {
        if (holyMeterUI == null)
            holyMeterUI = FindAnyObjectByType<HolyMeterUI>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        if (mainCamera != null && mainCamera.orthographic)
            originalCameraSize = mainCamera.orthographicSize;

        if (holyMeterUI != null)
            meterImage = holyMeterUI.GetComponentInChildren<Image>();

        if (meterImage != null)
            originalColor = meterImage.color;

        // 🔹 Restaura el progreso guardado desde el GameManager y actualiza la UI
        if (GameManager.Instance != null)
        {
            float savedProgress = GameManager.Instance.holyCharge;

            // Aplica el valor interno y actualiza visualmente la nueva UI
            if (holyMeterUI != null)
            {
                holyMeterUI.SetFillAmount(savedProgress);
                //holyMeterUI.UpdateFill(savedProgress);
            }

            // Aplica color si estaba completo
            if (savedProgress >= 1f)
            {
                isCharged = true;
                if (meterImage != null)
                    meterImage.color = glowColor;
            }
        }
    }

    // ============================================================
    // 🔹 Llamado por las balas cuando impactan enemigos
    // ============================================================
    public void RegisterEnemyHit(string tag)
    {
        if (holyMeterUI == null || !canGainEnergy || isCharged) return;

        holyMeterUI.AddProgress(progressPerHit);

        // Cuando se llena → queda listo para usarse
        if (holyMeterUI.IsFull() && !isCharged)
        {
            isCharged = true;
            if (meterImage != null)
                meterImage.color = glowColor;
        }

        // Si el golpe fue contra Boss o PolloLoco → aplicar cooldown
        if (tag == "Boss" || tag == "PolloLoco")
            StartCoroutine(BossCooldown());
    }

    private IEnumerator BossCooldown()
    {
        canGainEnergy = false;
        yield return new WaitForSeconds(bossHitCooldown);
        canGainEnergy = true;
    }

    // ============================================================
    // 🔹 Disparar el Holy Shot (llamado desde GunController)
    // ============================================================
    public bool IsCharged() => isCharged;

    public void TryActivateHolyShot()
    {
        if (!isCharged || isActive) return;

        isCharged = false;
        isActive = true;
        holyMeterUI.ResetMeter();

        if (meterImage != null)
            meterImage.color = originalColor;

        StartCoroutine(HolyShotSequence());
    }

    private IEnumerator HolyShotSequence()
    {
        // 🔸 Activa overlay
        if (overlayPanel != null)
            overlayPanel.SetActive(true);

        // 🔸 Zoom in
        yield return StartCoroutine(ZoomCamera(originalCameraSize, originalCameraSize * zoomFactor, transitionTime));

        // 🔸 Brillo pulsante mientras dura
        if (meterImage != null)
            StartCoroutine(PulseGlowEffect(holyShotDuration));

        // 🔸 Esperar duración total
        yield return new WaitForSeconds(holyShotDuration);

        // 🔸 Desactivar overlay
        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        // 🔸 Zoom out
        yield return StartCoroutine(ZoomCamera(mainCamera.orthographicSize, originalCameraSize, transitionTime));

        isActive = false;
    }

    private IEnumerator ZoomCamera(float from, float to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            mainCamera.orthographicSize = Mathf.Lerp(from, to, t);
            yield return null;
        }
    }

    private IEnumerator PulseGlowEffect(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            if (meterImage != null)
                meterImage.color = Color.Lerp(originalColor, glowColor, pulse);
            yield return null;
        }
    }

    public bool IsActive() => isActive;

    // ============================================================
    // 🔹 Métodos para GameManager (solo progreso de carga)
    // ============================================================
    public float GetCurrentCharge()
    {
        if (holyMeterUI != null)
            return holyMeterUI.GetFillAmount(); // valor entre 0 y 1
        return 0f;
    }

    public void SetCurrentCharge(float value)
    {
        if (holyMeterUI != null)
        {
            holyMeterUI.SetFillAmount(Mathf.Clamp01(value));
            //holyMeterUI.UpdateFill(Mathf.Clamp01(value));
        }
    }
}
