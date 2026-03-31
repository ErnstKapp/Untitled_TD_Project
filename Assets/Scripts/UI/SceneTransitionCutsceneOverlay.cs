using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Global full-screen transition overlay for scene changes:
/// fade image in -> wait (or click anywhere) -> load scene -> fade image out.
/// </summary>
public class SceneTransitionCutsceneOverlay : MonoBehaviour
{
    private static SceneTransitionCutsceneOverlay instance;

    private Canvas canvas;
    private Image image;
    private CanvasGroup canvasGroup;
    private bool isRunning;

    private static SceneTransitionCutsceneOverlay EnsureInstance()
    {
        if (instance != null) return instance;

        GameObject go = new GameObject("SceneTransitionCutsceneOverlay");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneTransitionCutsceneOverlay>();
        instance.BuildUI();
        return instance;
    }

    private void BuildUI()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        gameObject.AddComponent<GraphicRaycaster>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject imageGO = new GameObject("CutsceneImage");
        imageGO.transform.SetParent(transform, false);
        image = imageGO.AddComponent<Image>();
        image.raycastTarget = true;
        image.color = new Color(1f, 1f, 1f, 0f);

        RectTransform rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static void PlayAndLoad(string sceneName, Sprite sprite, float fadeInSeconds, float holdSeconds, float fadeOutSeconds, bool clickAnywhereToContinue)
    {
        if (string.IsNullOrEmpty(sceneName) || sprite == null)
            return;

        SceneTransitionCutsceneOverlay overlay = EnsureInstance();
        if (overlay.isRunning)
            return;

        overlay.StartCoroutine(overlay.PlayRoutine(sceneName, sprite, fadeInSeconds, holdSeconds, fadeOutSeconds, clickAnywhereToContinue));
    }

    private IEnumerator PlayRoutine(string sceneName, Sprite sprite, float fadeInSeconds, float holdSeconds, float fadeOutSeconds, bool clickAnywhereToContinue)
    {
        isRunning = true;

        image.sprite = sprite;
        SetAlpha(0f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f, Mathf.Max(0f, fadeInSeconds));

        // Wait either delay completion or click-anywhere, whichever comes first if click is enabled.
        if (clickAnywhereToContinue)
        {
            float t = 0f;
            while (t < holdSeconds)
            {
                if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
                    break;
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        else if (holdSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(holdSeconds);
        }

        SceneManager.LoadScene(sceneName);
        // Give scene one frame to settle before fading out.
        yield return null;

        yield return Fade(1f, 0f, Mathf.Max(0f, fadeOutSeconds));

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        SetAlpha(0f);
        isRunning = false;
    }

    private IEnumerator Fade(float from, float to, float duration)
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
        Color c = image.color;
        c.a = a;
        image.color = c;
        canvasGroup.alpha = a;
    }
}

