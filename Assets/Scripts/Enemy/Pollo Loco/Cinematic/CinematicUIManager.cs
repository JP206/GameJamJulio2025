using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CinematicUIManager : MonoBehaviour
{
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Slider enemyHpBar;
    [SerializeField] private float fillDuration = 2f;

    private void Start()
    {
        // 🔹 Asegurar que la barra empiece en 0 al inicio de la escena
        ResetBossHP();
    }

    public void ResetBossHP()
    {
        if (enemyHpBar != null)
        {
            enemyHpBar.value = 0f;
        }
    }

    public IEnumerator FillBossHP()
    {
        if (uiCanvas) uiCanvas.gameObject.SetActive(true);
        if (enemyHpBar == null) yield break;

        enemyHpBar.value = 0f;
        float elapsed = 0f;
        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            enemyHpBar.value = Mathf.Lerp(0f, enemyHpBar.maxValue, elapsed / fillDuration);
            yield return null;
        }

        enemyHpBar.value = enemyHpBar.maxValue;
    }
}
