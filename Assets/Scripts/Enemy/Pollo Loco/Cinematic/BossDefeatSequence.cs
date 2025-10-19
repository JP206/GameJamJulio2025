using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class BossDefeatSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinematicCameraFocus cinematicCameraFocus;
    [SerializeField] private UICanvasManager uiManager;
    [SerializeField] public GameObject canvasWin;

    [Header("Timings")]
    [SerializeField] private float focusDelay = 2f;

    private GameObject player;

    public void StartDefeatSequence()
    {
        StartCoroutine(DefeatSequenceCoroutine());
    }

    private IEnumerator DefeatSequenceCoroutine()
    {
        // 🧍 Obtener referencia al jugador
        player = GameObject.FindGameObjectWithTag("Player");

        // 🚫 Bloquear controles del jugador
        if (player != null)
        {
            var move = player.GetComponent<PlayerMovement>();
            var gun = player.GetComponent<_GunController>();
            var input = player.GetComponent<PlayerInput>();

            if (move) move.enabled = false;
            if (gun) gun.enabled = false;
            if (input) input.enabled = false;

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // 🧩 Desactivar toda la UI visible
        if (uiManager != null)
        {
            uiManager.enabled = false;
            if (uiManager.gameObject.activeSelf)
                uiManager.gameObject.SetActive(false);
        }

        // 🧩 Desactivar todos los Canvas excepto el de victoria
        var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in allCanvases)
        {
            if (c.gameObject.name != "CanvasWIN")
                c.gameObject.SetActive(false);
        }

        // 🎥 Enfocar cámara en el jefe
        if (cinematicCameraFocus != null)
            yield return StartCoroutine(cinematicCameraFocus.FocusOnBoss());

        yield return new WaitForSeconds(focusDelay);

        // 🏆 Mostrar Canvas de victoria
        if (canvasWin != null)
        {
            canvasWin.SetActive(true);
        }

        // 🖱️ Restaurar cursor visible
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // 👇 Método auxiliar existente
    public IEnumerator StartFocusOnly()
    {
        if (cinematicCameraFocus != null)
        {
            yield return StartCoroutine(cinematicCameraFocus.FocusOnBoss());
        }
    }

    // 👇 Nuevo: enfoque al jugador cuando el boss se desactiva
    public void FocusOnPlayerAfterBossDefeated()
    {
        StartCoroutine(FocusPlayerCoroutine());
    }

    private IEnumerator FocusPlayerCoroutine()
    {
        yield return new WaitForSeconds(0.5f); // pequeño delay tras el fade del boss

        if (cinematicCameraFocus != null)
            yield return StartCoroutine(cinematicCameraFocus.FocusOnPlayer());
    }
}
