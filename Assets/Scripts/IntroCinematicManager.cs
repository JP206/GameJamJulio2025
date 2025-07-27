using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class IntroCinematicManager : MonoBehaviour
{
    public static bool IsCinematicPlaying { get; private set; } = true;

    [Header("References")]
    public GameObject player;
    public GameObject waveManager;
    public GameObject ulCanvas;
    public Transform entryPoint;
    public Transform targetPoint;
    public GameObject cinematicCanvas;

    [Header("Cinematic Settings")]
    public float moveSpeed = 10f;
    public float delayBeforeEnable = 2f;

    private PlayerMovement playerMovement;
    private GunController gunController;
    private PlayerInput playerInput;

    void Start()
    {
        if (cinematicCanvas != null) cinematicCanvas.SetActive(true);

        waveManager.SetActive(false);
        ulCanvas.SetActive(false);

        playerMovement = player.GetComponent<PlayerMovement>();
        gunController = player.GetComponent<GunController>();
        playerInput = player.GetComponent<PlayerInput>();

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (gunController != null)
            gunController.enabled = false;

        if (playerInput != null)
            playerInput.enabled = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        player.transform.position = entryPoint.position;

        StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
            animator.SetBool("isRunning", true);

        yield return MoveToPosition(player.transform, targetPoint.position);

        if (animator != null)
            animator.SetBool("isRunning", false);

        yield return new WaitForSeconds(delayBeforeEnable);

        waveManager.SetActive(true);
        ulCanvas.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (gunController != null)
            gunController.enabled = true;

        if (playerInput != null)
            playerInput.enabled = true;

        if (cinematicCanvas != null) cinematicCanvas.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        IsCinematicPlaying = false;
        Destroy(gameObject);
    }

    IEnumerator MoveToPosition(Transform obj, Vector3 targetPos)
    {
        while (Vector3.Distance(obj.position, targetPos) > 0.05f)
        {
            obj.position = Vector3.MoveTowards(obj.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
