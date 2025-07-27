using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChangeScenes : MonoBehaviour
{
    [SerializeField] private float delayBeforeSceneChange = 1f;
    [SerializeField] private GameObject objectToActivateAfterAnimation;

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(ChangeSceneAfterDelay(sceneName));
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator ChangeSceneAfterDelay(string sceneName)
    {
        yield return new WaitForSeconds(delayBeforeSceneChange);
        SceneManager.LoadScene(sceneName);
    }

    public void AnimateThenChangeScene(GameObject buttonObject, string sceneName)
    {
        StartCoroutine(AnimateAndChangeScene(buttonObject, sceneName));
    }

    private IEnumerator AnimateAndChangeScene(GameObject buttonObject, string sceneName)
    {
        yield return AnimateClickEffect(buttonObject);
        yield return new WaitForSeconds(delayBeforeSceneChange);
        SceneManager.LoadScene(sceneName);
    }

    public void AnimateOnly(GameObject buttonObject)
    {
        StartCoroutine(AnimateClickEffect(buttonObject));
    }

    public void AnimateThenActivate(GameObject buttonObject, GameObject objectToActivate)
    {
        StartCoroutine(AnimateAndActivate(buttonObject, objectToActivate));
    }

    private IEnumerator AnimateAndActivate(GameObject buttonObject, GameObject objectToActivate)
    {
        yield return AnimateClickEffect(buttonObject);
        yield return new WaitForSeconds(delayBeforeSceneChange);
        if (objectToActivate != null)
            objectToActivate.SetActive(true);
    }

    public void AnimateAndActivateFromSerialized(GameObject buttonObject)
    {
        StartCoroutine(AnimateAndActivate(buttonObject, objectToActivateAfterAnimation));
    }

    private IEnumerator AnimateClickEffect(GameObject buttonObject)
    {
        Vector3 originalScale = buttonObject.transform.localScale;
        Vector3 pressedScale = originalScale * 0.9f;

        buttonObject.transform.localScale = pressedScale;
        yield return new WaitForSeconds(0.1f);
        buttonObject.transform.localScale = originalScale;
    }
}
