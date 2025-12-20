using UnityEngine;
using System.Collections;

public class BossDefeatSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinematicCameraFocus cinematicCameraFocus;
    [SerializeField] private UICanvasManager uiManager;
    [SerializeField] public GameObject canvasWin;

    [Header("Timings")]
    [SerializeField] private float focusOnBossDelay = 1.5f;

    public void StartDefeatSequence()
    {
        StartCoroutine(DefeatSequenceCoroutine());
    }

    private IEnumerator DefeatSequenceCoroutine()
    {
        // 🖱️ Ocultar cursor durante cinemática
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 🧩 Desactivar UI Manager
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

        // 🎥 1. Enfocar cámara en el jefe (solo movimiento, sin efectos)
        if (cinematicCameraFocus != null)
            yield return StartCoroutine(cinematicCameraFocus.SimpleFocusOnBoss());

        // ⏳ 2. Esperar mientras el boss termina de desvanecerse
        yield return new WaitForSeconds(focusOnBossDelay);

        // 🎥 3. Volver la cámara al jugador CON todos los efectos (shake, zoom)
        if (cinematicCameraFocus != null)
            yield return StartCoroutine(cinematicCameraFocus.FocusOnPlayerWithEffects());

        // 🏆 4. Mostrar Canvas de victoria
        if (canvasWin != null)
        {
            canvasWin.SetActive(true);
        }

        // 🖱️ Restaurar cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
