using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UICanvasManager : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject gameOverCanvas;

    [Header("Game Over Fade")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float targetAlpha = 0.47f; // Alpha = 120/255

    private bool isPaused = false;

    public static bool IsGamePausedOrOver { get; private set; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !gameOverCanvas.activeSelf)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseCanvas.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;

        IsGamePausedOrOver = isPaused;
    }

    public void ResumeGame()
    {
        TogglePause();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        IsGamePausedOrOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        IsGamePausedOrOver = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void TriggerGameOver()
    {
        StartCoroutine(DelayedGameOver());
    }

    private IEnumerator DelayedGameOver()
    {
        yield return new WaitForSeconds(1f);

        gameOverCanvas.SetActive(true);
        IsGamePausedOrOver = true;

        if (fadePanel != null)
        {
            Color color = fadePanel.color;
            float startAlpha = 0f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                fadePanel.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            fadePanel.color = new Color(color.r, color.g, color.b, targetAlpha);
        }
    }
}
