using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private PlayerHealth playerHealth;

    private void Start()
    {
        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.CurrentHealth;

        playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
    }

    private void UpdateHealthBar(int current, int max)
    {
        Debug.Log("Dentro de Player HP");
        healthSlider.maxValue = max;
        healthSlider.value = current;
    }
}
