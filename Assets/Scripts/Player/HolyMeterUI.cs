using UnityEngine;
using UnityEngine.UI;

public class HolyMeterUI : MonoBehaviour
{
    [SerializeField] private Image holyChickenImage;
    [SerializeField] private float fillSpeed = 0.25f;

    private float currentFill = 0f;
    private float targetFill = 0f;

    public void AddProgress(float amount)
    {
        targetFill = Mathf.Clamp01(targetFill + amount);
    }

    private void Update()
    {
        if (holyChickenImage == null) return;

        currentFill = Mathf.Lerp(currentFill, targetFill, fillSpeed * Time.deltaTime * 60f);
        holyChickenImage.fillAmount = currentFill;
    }

    public void ResetMeter()
    {
        targetFill = 0f;
    }

    public bool IsFull() => Mathf.Approximately(targetFill, 1f);

    // 🔹 Métodos nuevos
    public void SetFillAmount(float value)
    {
        if (holyChickenImage == null)
        {
            Debug.LogError("[HolyMeterUI] holyChickenImage es NULL");
            return;
        }

        targetFill = Mathf.Clamp01(value);
        currentFill = targetFill;
        holyChickenImage.fillAmount = currentFill;

        Debug.Log($"[HolyMeterUI] SetFillAmount aplicado: {currentFill}");
    }

    public float GetFillAmount()
    {
        return currentFill;
    }

    public void UpdateUIFromGameManager(float savedValue)
    {
        Debug.Log($"[HolyMeterUI] UpdateUIFromGameManager -> valor recibido: {savedValue}");
        SetFillAmount(savedValue);
    }
}
