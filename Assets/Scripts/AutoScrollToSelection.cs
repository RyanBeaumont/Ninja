using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class AutoScrollToSelection : MonoBehaviour
{
    public float scrollSpeed = 10f;
    public float padding = 50f;

    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;

    private GameObject lastSelected;

    private float targetVerticalPosition;
    private float targetHorizontalPosition;

    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();

        viewport = scrollRect.viewport;
        content = scrollRect.content;

        targetVerticalPosition = scrollRect.verticalNormalizedPosition;
        targetHorizontalPosition = scrollRect.horizontalNormalizedPosition;
    }

    void Update()
    {
        if(GameManager.Instance.controllerMode == false) return;
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected != lastSelected)
        {
            lastSelected = selected;

            if (selected != null)
            {
                RectTransform selectedRect =
                    selected.GetComponent<RectTransform>();

                if (selectedRect != null &&
                    selectedRect.IsChildOf(content))
                {
                    ScrollTo(selectedRect);
                }
            }
        }

        scrollRect.verticalNormalizedPosition = Mathf.Lerp(
            scrollRect.verticalNormalizedPosition,
            targetVerticalPosition,
            Time.unscaledDeltaTime * scrollSpeed);

        scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
            scrollRect.horizontalNormalizedPosition,
            targetHorizontalPosition,
            Time.unscaledDeltaTime * scrollSpeed);
    }

    private void ScrollTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        Bounds contentBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(content);

        Bounds targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(content, target);

        Vector3 targetCenter = targetBounds.center;

        if (scrollRect.vertical)
        {
            float viewportHeight = viewport.rect.height;
            float contentHeight = contentBounds.size.y;

            if (contentHeight > viewportHeight)
            {
                float targetY = -targetCenter.y;

                // Apply padding
                targetY -= padding;

                float normalized = Mathf.Clamp01(
                    targetY / (contentHeight - viewportHeight));

                targetVerticalPosition = 1f - normalized;
            }
        }

        if (scrollRect.horizontal)
        {
            float viewportWidth = viewport.rect.width;
            float contentWidth = contentBounds.size.x;

            if (contentWidth > viewportWidth)
            {
                float targetX = targetCenter.x;

                float normalized =
                    Mathf.Clamp01(
                        targetX /
                        (contentWidth - viewportWidth));

                targetHorizontalPosition = normalized;
            }
        }
    }
}

