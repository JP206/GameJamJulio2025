using System.Collections;
using UnityEngine;

public class CinematicBossIntro : MonoBehaviour
{
    [SerializeField] private GameObject bossObject;
    [SerializeField] private string introAnimationName = "PolloLocoWins";
    [SerializeField] private string moveAnimationName = "PolloLocoMovement";
    [SerializeField] private float delayAfterHPFill = 1f; // Espera antes de activar IA

    public IEnumerator PlayBossIntro(CinematicCameraFocus cameraFocus, CinematicUIManager uiManager)
    {
        if (bossObject == null)
            yield break;

        // 🔸 BLOQUEAR MOUSE AL INICIO DE TODA LA CINEMÁTICA
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 🔸 Activar boss visualmente
        bossObject.SetActive(true);

        // 🔸 Obtener componentes necesarios
        PolloLocoController bossController = bossObject.GetComponent<PolloLocoController>();
        Animator anim = bossObject.GetComponent<Animator>();

        // 🔸 Desactivar IA mientras dura la cinemática
        if (bossController != null)
            bossController.isCinematicMode = true;

        // 🔸 Enfoque de cámara con callback: dispara animación cuando el boss entra en cuadro
        if (cameraFocus != null)
        {
            bool animationPlayed = false;

            yield return StartCoroutine(cameraFocus.FocusOnBoss(() =>
            {
                if (!animationPlayed && anim != null)
                {
                    anim.Play(introAnimationName, 0, 0f);
                    animationPlayed = true;
                }
            }));
        }

        // 🔸 Llenar la barra de HP del boss (tipo MegaMan)
        if (uiManager != null)
            yield return StartCoroutine(uiManager.FillBossHP());

        // 🔸 Pausa antes de liberar IA
        yield return new WaitForSeconds(delayAfterHPFill);

        // 🔸 Reproducir animación de movimiento base
        if (anim != null && !string.IsNullOrEmpty(moveAnimationName))
            anim.Play(moveAnimationName, 0, 0f);

        // 🔸 Reactivar IA del boss
        if (bossController != null)
            bossController.isCinematicMode = false;

        // 🔸 DESBLOQUEAR MOUSE AL FINAL DE LA CINEMÁTICA
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
