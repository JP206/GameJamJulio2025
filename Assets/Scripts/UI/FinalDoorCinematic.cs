using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class FinalDoorCinematic : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineCamera cineCam;
    [SerializeField] private Transform doorPoint;
    [SerializeField] private UIFinalDoor finalDoor;
    [SerializeField] private GameObject uiCanvas;

    [Header("Cinematic Settings")]
    [SerializeField] private float camMoveSpeed = 12f;
    [SerializeField] private Vector3 camOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float delayAtDoor = 2f;

    [Header("Camera Shake")]
    [SerializeField] private float shakeMagnitude = 0.25f;
    [SerializeField] private float shakeFrequency = 25f;
    [SerializeField] private float shakeDuration = 1.5f;

    // Internos
    private PlayerMovement playerMovement;
    private _GunController gunController;
    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private Animator playerAnimator;
    private Transform camTarget;
    private Transform playerTransform;
    private SpriteRenderer playerSprite;

    public static bool IsPlaying { get; private set; } = false;

    private void Awake()
    {
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
            gunController = player.GetComponent<_GunController>();
            playerInput = player.GetComponent<PlayerInput>();
            playerSprite = player.GetComponent<SpriteRenderer>();
            rb = player.GetComponent<Rigidbody2D>();
            playerAnimator = player.GetComponent<Animator>();
        }
    }

    public void PlayCinematicToDoor()
    {
        if (IsPlaying || cineCam == null || doorPoint == null || playerTransform == null)
            return;

        StartCoroutine(Co_Play());
    }

    private IEnumerator Co_Play()
    {
        IsPlaying = true;

        bool uiWasActive = uiCanvas != null && uiCanvas.activeSelf;
        if (uiCanvas != null) uiCanvas.SetActive(false);

        if (playerMovement != null) playerMovement.enabled = false;
        if (gunController != null) gunController.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isRunning", false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (playerSprite != null)
            playerSprite.flipX = false;

        if (camTarget == null)
        {
            var go = new GameObject("DoorCamTarget");
            camTarget = go.transform;
        }

        camTarget.position = playerTransform.position + camOffset;
        cineCam.Follow = camTarget;

        Vector3 targetPos = doorPoint.position + camOffset;
        while (Vector3.Distance(camTarget.position, targetPos) > 0.05f)
        {
            camTarget.position = Vector3.MoveTowards(camTarget.position, targetPos, camMoveSpeed * Time.deltaTime);
            yield return null;
        }

        if (finalDoor != null)
            finalDoor.ShowFinalDoor();

        Vector3 basePos = camTarget.position;

        float shakeTime = 0f;
        while (shakeTime < shakeDuration)
        {
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * 2f * shakeMagnitude;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * 2f * shakeMagnitude;

            camTarget.position = basePos + new Vector3(offsetX, offsetY, 0f);

            shakeTime += Time.deltaTime;
            yield return null;
        }

        if (delayAtDoor > shakeDuration)
            yield return new WaitForSeconds(delayAtDoor - shakeDuration);

        camTarget.position = targetPos;
        cineCam.Follow = playerTransform;

        if (playerMovement != null) playerMovement.enabled = true;
        if (gunController != null) gunController.enabled = true;
        if (playerInput != null) playerInput.enabled = true;

        if (uiCanvas != null && uiWasActive)
            uiCanvas.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        IsPlaying = false;
    }
}
