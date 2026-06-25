using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PopupMessage : MonoBehaviour
{
    public TMP_Text messageText;
    public RectTransform popupRect;
    public CanvasGroup canvasGroup;
    public float slideDistance = 120f;
    public float slideDuration = 0.22f;
    public float fadeDuration = 0.2f;
    public float displayDuration = 2.5f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Vector2 visiblePosition;
    Vector2 hiddenPosition;
    Coroutine currentCoroutine;

    void Awake()
    {
        if (popupRect == null) popupRect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (popupRect != null)
        {
            visiblePosition = popupRect.anchoredPosition;
            hiddenPosition = visiblePosition + Vector2.left * slideDistance;
            popupRect.anchoredPosition = hiddenPosition;
        }

        canvasGroup.alpha = 0f;
    }

    public void SetMessage(string message, Color color)
    {
        ShowMessage(message, color, displayDuration);
    }

    public void ShowMessage(string message, Color color, float duration)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }

        if (popupRect != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);
            visiblePosition = popupRect.anchoredPosition;
            hiddenPosition = visiblePosition + Vector2.left * slideDistance;
            popupRect.anchoredPosition = hiddenPosition;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(AnimatePopup(duration));
    }

    IEnumerator AnimatePopup(float duration)
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (popupRect != null) popupRect.anchoredPosition = hiddenPosition;

        if (popupRect != null)
            //yield return AnimatePosition(hiddenPosition, visiblePosition, slideDuration);

        yield return FadeTo(1f, fadeDuration);
        yield return new WaitForSeconds(duration);
        yield return FadeOut();

        currentCoroutine = null;
    }

    IEnumerator AnimatePosition(Vector2 start, Vector2 end, float duration)
    {
        if (popupRect == null || duration <= 0f)
        {
            if (popupRect != null) popupRect.anchoredPosition = end;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = easeCurve.Evaluate(t);
            popupRect.anchoredPosition = Vector2.LerpUnclamped(start, end, eased);
            yield return null;
        }

        popupRect.anchoredPosition = end;
    }

    IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (canvasGroup == null || duration <= 0f)
        {
            if (canvasGroup != null) canvasGroup.alpha = targetAlpha;
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    IEnumerator FadeOut()
    {
        yield return FadeTo(0f, fadeDuration);
    }
}
