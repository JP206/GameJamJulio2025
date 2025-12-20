using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ChangeScene : MonoBehaviour
{
    [Header("Modo de Transición")]
    [Tooltip("Si está activo, la transición comienza automáticamente. Si no, espera a que se llame StartTransition()")]
    public bool autoStart = true;

    [Header("Escena Destino")]
    [Tooltip("Nombre de la escena a cargar. Si está vacío, carga la siguiente del Build Settings")]
    public string targetSceneName = "";

    [Header("Duración antes de cambiar de escena")]
    public float delayBeforeTransition = 4f;

    [Header("Duración del fade out")]
    public float fadeDuration = 1.5f;

    [Header("Referencia al panel de fade (con Image)")]
    public Image fadePanel;

    private bool isTransitioning = false;

    private void Start()
    {
        if (fadePanel != null)
        {
            // Asegura que el panel comience transparente
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
        }

        if (autoStart)
        {
            StartCoroutine(TransitionToNextScene());
        }
    }

    // Método público para llamar desde un botón
    public void StartTransition()
    {
        if (!isTransitioning)
        {
            StartCoroutine(TransitionToNextScene());
        }
    }

    private IEnumerator TransitionToNextScene()
    {
        isTransitioning = true;

        // Espera el delay (solo si autoStart está activo, si es botón el delay es 0 o configurable)
        if (autoStart && delayBeforeTransition > 0)
        {
            yield return new WaitForSeconds(delayBeforeTransition);
        }

        // Fade out
        if (fadePanel != null)
        {
            float elapsed = 0f;
            Color c = fadePanel.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / fadeDuration);
                fadePanel.color = c;
                yield return null;
            }
        }

        // Cargar escena
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // Cargar escena específica
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            // Cargar siguiente escena del Build Settings
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("No hay más escenas en el Build Settings.");
            }
        }
    }
}