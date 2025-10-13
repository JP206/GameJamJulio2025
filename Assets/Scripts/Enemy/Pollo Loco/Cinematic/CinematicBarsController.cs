using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CinematicBarsController : MonoBehaviour
{
    [SerializeField] private GameObject cinematicCanvas;
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float barMoveDistance = 200f;

    public IEnumerator ShowBars()
    {
        cinematicCanvas.SetActive(true);
        yield return StartCoroutine(FadeBars(0f, 1f, true));
    }

    public IEnumerator HideBars()
    {
        yield return StartCoroutine(FadeBars(1f, 0f, false, disableAfter: true));
    }

    private IEnumerator FadeBars(float startAlpha, float endAlpha, bool moveIn, bool disableAfter = false)
    {
        float elapsed = 0f;
        CanvasRenderer topR = topBar.GetComponent<CanvasRenderer>();
        CanvasRenderer bottomR = bottomBar.GetComponent<CanvasRenderer>();

        Vector2 topStart = topBar.anchoredPosition;
        Vector2 bottomStart = bottomBar.anchoredPosition;

        if (moveIn)
        {
            topBar.anchoredPosition = topStart + Vector2.up * barMoveDistance;
            bottomBar.anchoredPosition = bottomStart - Vector2.up * barMoveDistance;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            topR.SetAlpha(alpha);
            bottomR.SetAlpha(alpha);

            if (moveIn)
            {
                topBar.anchoredPosition = Vector2.Lerp(topStart + Vector2.up * barMoveDistance, topStart, t);
                bottomBar.anchoredPosition = Vector2.Lerp(bottomStart - Vector2.up * barMoveDistance, bottomStart, t);
            }
            else
            {
                topBar.anchoredPosition = Vector2.Lerp(topStart, topStart + Vector2.up * barMoveDistance, t);
                bottomBar.anchoredPosition = Vector2.Lerp(bottomStart, bottomStart - Vector2.up * barMoveDistance, t);
            }

            yield return null;
        }

        if (disableAfter) cinematicCanvas.SetActive(false);
    }
}
