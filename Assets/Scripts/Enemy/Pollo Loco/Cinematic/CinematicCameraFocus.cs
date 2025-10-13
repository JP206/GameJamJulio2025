using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CinematicCameraFocus : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera cineCam;
    [SerializeField] private Transform bossFocusPoint;

    [Header("Timing")]
    [SerializeField] private float lerpSpeed = 2f;
    [SerializeField] private float focusDuration = 2.5f;
    [SerializeField] private float zoomAmount = 3f;
    [SerializeField] private float waitOnBoss = 1f;

    [Header("Shake Effect")]
    [SerializeField] private float shakeMagnitude = 0.25f;
    [SerializeField] private float shakeFrequency = 25f;
    [SerializeField] private float shakeDuration = 1.2f;

    private float originalZoom;
    private Transform originalFollow;
    private Vector3 originalPosition;

    public IEnumerator FocusOnBoss(Action onBossCentered = null)
    {
        if (cineCam == null || bossFocusPoint == null)
            yield break;

        // 🔸 Desactivar cursor durante la cinemática
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Guardar estado inicial
        originalFollow = cineCam.Follow;
        originalZoom = cineCam.Lens.OrthographicSize;
        originalPosition = cineCam.transform.position;

        cineCam.Follow = null;

        float elapsed = 0f;
        float startZoom = originalZoom;
        bool triggered = false;

        Vector3 targetPos = new Vector3(
            bossFocusPoint.position.x,
            bossFocusPoint.position.y,
            cineCam.transform.position.z
        );

        // 🔹 Movimiento suave + callback cuando el boss entra en cuadro
        while (elapsed < focusDuration)
        {
            elapsed += Time.deltaTime * lerpSpeed;

            cineCam.transform.position = Vector3.Lerp(originalPosition, targetPos, elapsed / focusDuration);
            cineCam.Lens.OrthographicSize = Mathf.Lerp(startZoom, startZoom - zoomAmount, elapsed / focusDuration);

            if (!triggered && elapsed / focusDuration >= 0.6f && onBossCentered != null)
            {
                onBossCentered.Invoke();
                triggered = true;
            }

            yield return null;
        }

        // 🔸 Cámara fija sobre el boss
        cineCam.transform.position = targetPos;

        // 🔹 Aplicar shake
        yield return StartCoroutine(ShakeCamera(targetPos));

        // 🔹 Esperar un momento sobre el boss
        yield return new WaitForSeconds(waitOnBoss);

        // 🔹 Regresar al jugador
        elapsed = 0f;
        while (elapsed < focusDuration)
        {
            elapsed += Time.deltaTime * lerpSpeed;
            cineCam.transform.position = Vector3.Lerp(targetPos, originalPosition, elapsed / focusDuration);
            cineCam.Lens.OrthographicSize = Mathf.Lerp(startZoom - zoomAmount, startZoom, elapsed / focusDuration);
            yield return null;
        }

        // Restaurar cámara
        cineCam.Follow = originalFollow;
        cineCam.Lens.OrthographicSize = originalZoom;
        cineCam.transform.position = originalPosition;

        // 🔸 Restaurar cursor después de la cinemática
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator ShakeCamera(Vector3 center)
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * shakeMagnitude;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency + 1f) - 0.5f) * 2f * shakeMagnitude;

            cineCam.transform.position = center + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cineCam.transform.position = center;
    }
}
