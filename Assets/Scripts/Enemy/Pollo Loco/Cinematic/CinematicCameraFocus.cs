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

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

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

        cineCam.transform.position = targetPos;

        yield return StartCoroutine(ShakeCamera(targetPos));

        yield return new WaitForSeconds(waitOnBoss);

        elapsed = 0f;
        while (elapsed < focusDuration)
        {
            elapsed += Time.deltaTime * lerpSpeed;
            cineCam.transform.position = Vector3.Lerp(targetPos, originalPosition, elapsed / focusDuration);
            cineCam.Lens.OrthographicSize = Mathf.Lerp(startZoom - zoomAmount, startZoom, elapsed / focusDuration);
            yield return null;
        }

        cineCam.Follow = originalFollow;
        cineCam.Lens.OrthographicSize = originalZoom;
        cineCam.transform.position = originalPosition;

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

    public IEnumerator FocusOnPlayer()
    {
        if (cineCam == null)
            yield break;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            yield break;

        // Guardamos estado original
        Transform originalFollow = cineCam.Follow;
        float originalZoom = cineCam.Lens.OrthographicSize;
        Vector3 originalPosition = cineCam.transform.position;

        cineCam.Follow = null;

        // === 1️⃣ Transición hasta el jugador ===
        Vector3 startPos = cineCam.transform.position;
        Vector3 targetPos = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            cineCam.transform.position.z
        );

        float startZoom = cineCam.Lens.OrthographicSize;
        float elapsed = 0f;
        float duration = focusDuration; // usa el mismo tiempo que el boss

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime * lerpSpeed;
            cineCam.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            cineCam.Lens.OrthographicSize = Mathf.Lerp(startZoom, startZoom - zoomAmount, elapsed / duration);
            yield return null;
        }

        cineCam.transform.position = targetPos;
        cineCam.Lens.OrthographicSize = startZoom - zoomAmount;

        // === 2️⃣ Shake y pausa en la posición del jugador ===
        yield return StartCoroutine(ShakeCamera(targetPos));
        yield return new WaitForSeconds(waitOnBoss);

        // === 3️⃣ Mostrar Canvas WIN aquí mismo ===
        var defeatSequence = FindAnyObjectByType<BossDefeatSequence>();
        if (defeatSequence != null && defeatSequence.canvasWin != null)
        {
            defeatSequence.canvasWin.SetActive(true);
        }

        // 🔹 Mantenemos la cámara centrada en el jugador (no vuelve atrás)
        cineCam.Follow = null;
        cineCam.transform.position = targetPos;
        cineCam.Lens.OrthographicSize = startZoom - zoomAmount;
    }

}
