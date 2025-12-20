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

    /// <summary>
    /// Focus simple en el boss - solo mueve la cámara, sin efectos
    /// </summary>
    public IEnumerator SimpleFocusOnBoss()
    {
        if (cineCam == null || bossFocusPoint == null)
            yield break;

        // Guardar estado original
        originalFollow = cineCam.Follow;
        originalZoom = cineCam.Lens.OrthographicSize;
        originalPosition = cineCam.transform.position;

        cineCam.Follow = null;

        Vector3 targetPos = new Vector3(
            bossFocusPoint.position.x,
            bossFocusPoint.position.y,
            cineCam.transform.position.z
        );

        // Mover cámara al boss con zoom
        float elapsed = 0f;
        float startZoom = originalZoom;

        while (elapsed < focusDuration)
        {
            elapsed += Time.deltaTime * lerpSpeed;
            cineCam.transform.position = Vector3.Lerp(originalPosition, targetPos, elapsed / focusDuration);
            cineCam.Lens.OrthographicSize = Mathf.Lerp(startZoom, startZoom - zoomAmount, elapsed / focusDuration);
            yield return null;
        }

        cineCam.transform.position = targetPos;
        cineCam.Lens.OrthographicSize = startZoom - zoomAmount;
    }

    /// <summary>
    /// Focus en el jugador CON todos los efectos (shake, zoom) y la cámara lo sigue al final
    /// </summary>
    public IEnumerator FocusOnPlayerWithEffects()
    {
        if (cineCam == null)
            yield break;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            yield break;

        Vector3 startPos = cineCam.transform.position;
        Vector3 targetPos = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            cineCam.transform.position.z
        );

        float startZoom = cineCam.Lens.OrthographicSize;
        float elapsed = 0f;

        // 1️⃣ Mover cámara hacia el jugador
        while (elapsed < focusDuration)
        {
            elapsed += Time.deltaTime * lerpSpeed;
            cineCam.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / focusDuration);
            yield return null;
        }

        cineCam.transform.position = targetPos;

        // 2️⃣ Shake en la posición del jugador
        yield return StartCoroutine(ShakeCamera(targetPos));

        // 3️⃣ Esperar un momento
        yield return new WaitForSeconds(waitOnBoss);

        // 4️⃣ Restaurar zoom y hacer que la cámara siga al jugador
        cineCam.Lens.OrthographicSize = originalZoom;
        cineCam.Follow = player.transform;
    }

    /// <summary>
    /// Focus completo en el boss con callback (usado en intro del boss)
    /// </summary>
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

    /// <summary>
    /// Focus simple en el jugador (usado en otros contextos)
    /// </summary>
    public IEnumerator FocusOnPlayer()
    {
        if (cineCam == null)
            yield break;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            yield break;

        cineCam.Follow = null;

        Vector3 startPos = cineCam.transform.position;
        Vector3 targetPos = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            cineCam.transform.position.z
        );

        float elapsed = 0f;

        while (elapsed < focusDuration)
        {
            elapsed += Time.deltaTime * lerpSpeed;
            cineCam.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / focusDuration);
            yield return null;
        }

        cineCam.transform.position = targetPos;
        
        yield return new WaitForSeconds(waitOnBoss * 0.5f);

        cineCam.Follow = player.transform;
        cineCam.Lens.OrthographicSize = originalZoom;
    }
}
