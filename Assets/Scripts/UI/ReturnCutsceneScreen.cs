using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene-local full-screen image fade player for short return cutscenes.
/// Add this to your Overworld canvas and assign a full-screen Image.
/// </summary>
public class ReturnCutsceneScreen : MonoBehaviour
{
    [SerializeField] private Image cutsceneImage;

    private void Awake()
    {
        HideImmediate();
    }

    public void HideImmediate()
    {
        if (cutsceneImage == null) return;
        Color c = cutsceneImage.color;
        c.a = 0f;
        cutsceneImage.color = c;
        cutsceneImage.enabled = false;
        cutsceneImage.raycastTarget = false;
    }

    public IEnumerator Play(Sprite sprite, float fadeInSeconds, float holdSeconds, float fadeOutSeconds)
    {
        if (cutsceneImage == null || sprite == null)
            yield break;

        cutsceneImage.enabled = true;
        cutsceneImage.raycastTarget = true;
        cutsceneImage.sprite = sprite;

        yield return FadeAlpha(0f, 1f, Mathf.Max(0f, fadeInSeconds));

        if (holdSeconds > 0f)
            yield return new WaitForSecondsRealtime(holdSeconds);

        yield return FadeAlpha(1f, 0f, Mathf.Max(0f, fadeOutSeconds));

        cutsceneImage.raycastTarget = false;
        cutsceneImage.enabled = false;
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float t = 0f;
        SetAlpha(from);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            SetAlpha(Mathf.Lerp(from, to, p));
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float a)
    {
        if (cutsceneImage == null) return;
        Color c = cutsceneImage.color;
        c.a = a;
        cutsceneImage.color = c;
    }
}
