using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FinalBossCinematic : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private GameObject portalObject;
    [SerializeField] private GameObject squareObject;
    [SerializeField] private GameObject triangleObject;
    [SerializeField] private GameObject bossDoor;

    [Header("Player")]
    [SerializeField] private Transform playerTargetPoint;
    [SerializeField] private float walkDuration = 1.5f;

    [Header("Timings")]
    [SerializeField] private float delayBeforeWalk = 1f;
    [SerializeField] private float delayTriangle = 0.5f;
    [SerializeField] private float delaySquare = 0.5f;
    [SerializeField] private float delayPortal = 1f;
    [SerializeField] private float delayDoor = 1.5f;
    [SerializeField] private float delayBeforeBossActive = 1f;

    [Header("Boss Activation")]
    [SerializeField] private GameObject bossObject;

    [Header("UI Elements")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Image polloLocoBackground;
    [SerializeField] private Slider enemyHpBar; // 🔹 ahora es Slider
    [SerializeField] private Image polloLocoText;
    [SerializeField] private float fadeDuration = 1.5f;

    private GameObject player;

    private IEnumerator Start()
    {
        if (uiCanvas != null)
            uiCanvas.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.3f);

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        var move = player.GetComponent<PlayerMovement>();
        var gun = player.GetComponent<_GunController>();
        var input = player.GetComponent<UnityEngine.InputSystem.PlayerInput>();

        if (move) move.enabled = false;
        if (gun) gun.enabled = false;
        if (input) input.enabled = false;

        SetActiveIfNotNull(triangleObject, true);
        SetActiveIfNotNull(squareObject, true);
        SetActiveIfNotNull(portalObject, true);
        SetActiveIfNotNull(bossDoor, true);

        yield return new WaitForSeconds(delayBeforeWalk);

        FaceTarget(player, playerTargetPoint);
        yield return StartCoroutine(PlayerWalkToTarget());
        yield return StartCoroutine(CloseSequence());

        yield return new WaitForSeconds(delayBeforeBossActive);
        if (bossObject != null) bossObject.SetActive(true);

        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(true);
            StartCoroutine(FadeInBossUI());
        }

        if (move) move.enabled = true;
        if (gun) gun.enabled = true;
        if (input) input.enabled = true;
    }

    private IEnumerator PlayerWalkToTarget()
    {
        if (player == null || playerTargetPoint == null)
            yield break;

        Vector3 startPos = player.transform.position;
        Vector3 endPos = playerTargetPoint.position;

        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.SetBool("isRunning", true);

        float elapsed = 0f;
        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / walkDuration);
            player.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        if (anim != null) anim.SetBool("isRunning", false);
    }

    private IEnumerator CloseSequence()
    {
        if (triangleObject != null)
        {
            Animator triangleAnim = triangleObject.GetComponent<Animator>();
            if (triangleAnim != null)
                triangleAnim.Play("TriangleDisappear", 0, 0f);
            yield return new WaitForSeconds(delayTriangle);
            triangleObject.SetActive(false);
        }

        if (squareObject != null)
        {
            Animator squareAnim = squareObject.GetComponent<Animator>();
            if (squareAnim != null)
                squareAnim.Play("SquareDisappear", 0, 0f);
            yield return new WaitForSeconds(delaySquare);
            squareObject.SetActive(false);
        }

        if (portalAnimator != null)
        {
            portalAnimator.Play("PortalClose", 0, 0f);
            yield return new WaitForSeconds(delayPortal);
        }
        if (portalObject != null)
            portalObject.SetActive(false);

        if (doorAnimator != null)
        {
            doorAnimator.Play("Close", 0, 0f);
            yield return new WaitForSeconds(delayDoor);
        }
        if (bossDoor != null)
            bossDoor.SetActive(false);
    }

    private void FaceTarget(GameObject playerObj, Transform target)
    {
        if (playerObj == null || target == null)
            return;

        SpriteRenderer sr = playerObj.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.flipX = target.position.x < playerObj.transform.position.x;
    }

    private void SetActiveIfNotNull(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }

    // ✅ Fade in adaptado para que el EnemyHP_Bar sea un Slider
    private IEnumerator FadeInBossUI()
    {
        float elapsed = 0f;
        float hpBarDelay = 0.5f; // 🕒 pequeño retardo antes de la barra

        // 🔹 Fades del texto y fondo
        if (polloLocoBackground) polloLocoBackground.canvasRenderer.SetAlpha(0f);
        if (polloLocoText) polloLocoText.canvasRenderer.SetAlpha(0f);

        // 🔹 Obtener imágenes internas del Slider
        Image hpEmptyImage = null;
        Image hpFillImage = null;

        if (enemyHpBar != null)
        {
            if (enemyHpBar.GetComponentInChildren<CanvasRenderer>())
            {
                Transform emptyBar = enemyHpBar.transform.Find("EmptyBar");
                Transform fillArea = enemyHpBar.transform.Find("Fill Area");

                if (emptyBar)
                    hpEmptyImage = emptyBar.GetComponent<Image>();
                if (fillArea && fillArea.Find("Fill"))
                    hpFillImage = fillArea.Find("Fill").GetComponent<Image>();
            }
        }

        // 🔹 Inicializar ambos invisibles
        if (hpEmptyImage)
        {
            Color c = hpEmptyImage.color;
            c.a = 0f;
            hpEmptyImage.color = c;
        }
        if (hpFillImage)
        {
            Color c = hpFillImage.color;
            c.a = 0f;
            hpFillImage.color = c;
        }

        // 🔹 Fade del texto y fondo
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            if (polloLocoBackground) polloLocoBackground.canvasRenderer.SetAlpha(alpha);
            if (polloLocoText) polloLocoText.canvasRenderer.SetAlpha(alpha);

            yield return null;
        }

        // 🔹 Esperar antes de la barra
        yield return new WaitForSeconds(hpBarDelay);

        // 🔹 Fade de la barra (Empty + Fill)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            if (hpEmptyImage)
            {
                Color c = hpEmptyImage.color;
                c.a = alpha;
                hpEmptyImage.color = c;
            }

            if (hpFillImage)
            {
                Color c = hpFillImage.color;
                c.a = alpha;
                hpFillImage.color = c;
            }

            yield return null;
        }

        // 🔹 Asegurar visibilidad total
        if (polloLocoBackground) polloLocoBackground.canvasRenderer.SetAlpha(1f);
        if (polloLocoText) polloLocoText.canvasRenderer.SetAlpha(1f);

        if (hpEmptyImage)
        {
            Color c = hpEmptyImage.color;
            c.a = 1f;
            hpEmptyImage.color = c;
        }

        if (hpFillImage)
        {
            Color c = hpFillImage.color;
            c.a = 1f;
            hpFillImage.color = c;
        }
    }
}
