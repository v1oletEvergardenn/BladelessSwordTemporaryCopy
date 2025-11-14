using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class BorderAutoResizer : MonoBehaviour
{
    [Tooltip("Extra height to add to the border (e.g. for padding)")]
    public float offset = 0f;

    [Tooltip("Maximum height for the border. Set to 0 for unlimited.")]
    public float maxHeight = 0f;

    private RectTransform borderRect;

    private void Awake()
    {
        EnsureRectTransform();
        SetPivotAndAnchorsTop();
        EnforceChildrenTopAnchors();
    }

    private void OnEnable()
    {
        EnsureRectTransform();
        SetPivotAndAnchorsTop();
        EnforceChildrenTopAnchors();
        ResizeBorder();
    }

    private void Update()
    {
        EnforceChildrenTopAnchors();
        ResizeBorder();
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        EnsureRectTransform();
        SetPivotAndAnchorsTop();
        EnforceChildrenTopAnchors();
        ResizeBorder();
    }

#endif

    private void EnsureRectTransform()
    {
        if (borderRect == null)
            borderRect = GetComponent<RectTransform>();
    }

    private void SetPivotAndAnchorsTop()
    {
        if (borderRect == null) return;
        borderRect.pivot = new Vector2(borderRect.pivot.x, 1f);
        borderRect.anchorMin = new Vector2(borderRect.anchorMin.x, 1f);
        borderRect.anchorMax = new Vector2(borderRect.anchorMax.x, 1f);
    }

    private void EnforceChildrenTopAnchors()
    {
        if (borderRect == null) return;
        foreach (RectTransform child in GetAllDescendants(borderRect))
        {
            child.pivot = new Vector2(child.pivot.x, 1f);
            child.anchorMin = new Vector2(child.anchorMin.x, 1f);
            child.anchorMax = new Vector2(child.anchorMax.x, 1f);

            Vector3 pos = child.localPosition;
            if (pos.y > 0f)
            {
                pos.y = 0f;
                child.localPosition = pos;
            }
        }
    }

    private void ResizeBorder()
    {
        if (borderRect == null) return;

        bool first = true;
        float topMost = 0f;
        float bottomMost = 0f;

        foreach (RectTransform child in GetAllDescendants(borderRect))
        {
            if (!child.gameObject.activeSelf)
                continue;

            // Calculate the top and bottom in local space, considering pivot
            float childTop = child.localPosition.y + (1 - child.pivot.y) * child.rect.height;
            float childBottom = child.localPosition.y - child.pivot.y * child.rect.height;

            if (first)
            {
                topMost = childTop;
                bottomMost = childBottom;
                first = false;
            }
            else
            {
                if (childTop > topMost) topMost = childTop;
                if (childBottom < bottomMost) bottomMost = childBottom;
            }
        }

        float totalHeight = Mathf.Abs(topMost - bottomMost) + offset;
        if (maxHeight > 0f)
            totalHeight = Mathf.Min(totalHeight, maxHeight);

        var size = borderRect.sizeDelta;
        size.y = totalHeight;
        borderRect.sizeDelta = size;
    }

    // Recursively get all active RectTransform descendants (excluding self)
    private static IEnumerable<RectTransform> GetAllDescendants(RectTransform parent)
    {
        foreach (Transform child in parent)
        {
            if (child is RectTransform rectChild)
            {
                yield return rectChild;
                foreach (var descendant in GetAllDescendants(rectChild))
                    yield return descendant;
            }
        }
    }
}