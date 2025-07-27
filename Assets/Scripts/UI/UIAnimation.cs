using System.Collections;
using TMPro;
using UnityEngine;

public class UIAnimation : MonoBehaviour
{
    float duration = 0.2f;
    bool running = false;

    private readonly Color32 red = new Color(1, 0, 0, 1), yellow = new Color(1, 0.85f, 0, 1), 
        orange = new Color(1, 0.48f, 0, 1), white = new Color(1, 1, 1, 1);

    private readonly Vector2 originalScale = new(1, 1), targetScale = new Vector2(1.5f, 1.5f);
    private readonly Quaternion originalRotation = Quaternion.identity, leftRotation = Quaternion.Euler(0f, 0f, 15f),
        rightRotation = Quaternion.Euler(0f, 0f, -15f);

    public void Animation(TextMeshProUGUI text)
    {
        if (!running)
        {
            StartCoroutine(animation(text));
        }
    }

    IEnumerator animation(TextMeshProUGUI text)
    {
        running = true;

        ChangeTextColor(text);
        float time = 0;
        int random = Random.Range(0, 2);
        Quaternion targetRotation = (random == 1) ? leftRotation : rightRotation;

        while (time < duration)
        {
            transform.localScale = Vector2.Lerp(originalScale, targetScale, time);
            transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, time);
            yield return null;
            time += Time.deltaTime;
        }

        time = 0;

        while (time < duration)
        {
            transform.localScale = Vector2.Lerp(targetScale, originalScale, time);
            transform.rotation = Quaternion.Lerp(targetRotation, originalRotation, time);
            yield return null;
            time += Time.deltaTime;
        }

        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        ResetTextoColor(text);

        running = false;
    }

    void ChangeTextColor(TextMeshProUGUI text)
    {
        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                text.color = red;
                break;
            case 1:
                text.color = orange;
                break;
            case 2:
                text.color = yellow;
                break;
        }
    }

    void ResetTextoColor(TextMeshProUGUI text)
    {
        text.color = white;
    }
}
