using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ChangeScene : MonoBehaviour
{
    [Header("Duración antes de cambiar de escena")]
    public float delayBeforeTransition = 4f;

    [Header("Duración del fade out")]
    public float fadeDuration = 1.5f;

    [Header("Referencia al panel de fade (con Image)")]
    public Image fadePanel;

    private void Start()
    {
        if (fadePanel != null)
        {
            // Asegura que el panel comience transparente
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
        }

        StartCoroutine(TransitionToNextScene());
    }

    private IEnumerator TransitionToNextScene()
    {
        // Espera los 4 segundos
        yield return new WaitForSeconds(delayBeforeTransition);

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

        // Carga la siguiente escena del Build Settings
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