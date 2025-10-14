using System.Collections;
using UnityEngine;

public class CinematicBossIntro : MonoBehaviour
{
    [SerializeField] private GameObject bossObject;
    [SerializeField] private string idleAnimationName = "PolloLocoIdle";
    [SerializeField] private string warCryAnimationName = "PolloLocoWarCry";
    [SerializeField] private string moveAnimationName = "PolloLocoMovement"; // 🔹 movimiento base real
    [SerializeField] private float delayAfterHPFill = 1f;

    public IEnumerator PlayBossIntro(CinematicCameraFocus cameraFocus, CinematicUIManager uiManager)
    {
        if (bossObject == null)
            yield break;

        // 🔒 Bloquear mouse desde el inicio
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        bossObject.SetActive(true);

        PolloLocoController bossController = bossObject.GetComponent<PolloLocoController>();
        Animator anim = bossObject.GetComponent<Animator>();

        if (bossController != null)
            bossController.isCinematicMode = true;

        // 🔸 Empieza en Idle
        if (anim != null && !string.IsNullOrEmpty(idleAnimationName))
            anim.Play(idleAnimationName, 0, 0f);

        // 🔸 Enfoque de cámara → WarCry cuando el boss entra en cuadro
        if (cameraFocus != null)
        {
            bool warCryPlayed = false;

            yield return StartCoroutine(cameraFocus.FocusOnBoss(() =>
            {
                if (!warCryPlayed && anim != null)
                {
                    anim.Play(warCryAnimationName, 0, 0f);
                    warCryPlayed = true;
                }
            }));
        }

        // 🔸 Llenado de barra de HP
        if (uiManager != null)
            yield return StartCoroutine(uiManager.FillBossHP());

        // 🔸 🔹 Al terminar el HP Fill → vuelve a Idle antes del movimiento real
        if (anim != null && !string.IsNullOrEmpty(idleAnimationName))
            anim.Play(idleAnimationName, 0, 0f);

        // 🔸 Espera y luego arranca la animación de movimiento
        yield return new WaitForSeconds(delayAfterHPFill);

        if (anim != null && !string.IsNullOrEmpty(moveAnimationName))
            anim.Play(moveAnimationName, 0, 0f);

        // 🔸 Reactivar IA
        if (bossController != null)
            bossController.isCinematicMode = false;

        // 🔓 Restaurar mouse
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
