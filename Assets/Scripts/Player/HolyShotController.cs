using UnityEngine;
using System.Collections;

public class HolyShotController : MonoBehaviour
{
    [SerializeField] private GameObject overlayPanel;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomFactor = 0.9f;
    [SerializeField] private float transitionTime = 0.2f;

    private float originalCameraSize;
    private bool isActive = false;

    void Start()
    {
        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera.orthographic)
            originalCameraSize = mainCamera.orthographicSize;
    }

    public void StartHolyShot(float duration)
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(HolyShotSequence(duration));
    }

    private IEnumerator HolyShotSequence(float duration)
    {
        if (overlayPanel != null)
            overlayPanel.SetActive(true);

        yield return StartCoroutine(ZoomCamera(originalCameraSize, originalCameraSize * zoomFactor, transitionTime));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yield return new WaitForSeconds(duration);

        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        yield return StartCoroutine(ZoomCamera(mainCamera.orthographicSize, originalCameraSize, transitionTime));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
}
