using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DialogueBubbleAutoSize : MonoBehaviour
{
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private TMP_Text textMeshPro;

    [Header("Size")]
    [OnValueChanged("RefreshSize")][SerializeField] private float minWidth = 80f;

    [OnValueChanged("RefreshSize")][SerializeField] private float maxWidth = 420f;

    private string lastText = string.Empty;

    private void Reset()
    {
        if (bubbleRect == null) bubbleRect = GetComponent<RectTransform>();
        if (textMeshPro == null) textMeshPro = GetComponentInChildren<TMP_Text>(true);
    }

    private void OnEnable()
    {
        RefreshSize(true);
    }

    private void LateUpdate()
    {
        RefreshSize(false);
    }

    private void RefreshSize(bool force = true)
    {
        var currentText = GetCurrentText();
        if (!force && currentText == lastText) return;
        lastText = currentText;

        var preferred = GetPreferredTextSize(currentText);
        var width = Mathf.Clamp(preferred.x, minWidth, maxWidth);
        var preferredWrapped = GetPreferredTextSize(currentText, width);
        var height = preferredWrapped.y;

        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        bubbleRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        LayoutRebuilder.ForceRebuildLayoutImmediate(bubbleRect);
    }

    private string GetCurrentText()
    {
        if (textMeshPro != null) return textMeshPro.text ?? string.Empty;
        return string.Empty;
    }

    private Vector2 GetPreferredTextSize(string content, float constrainedWidth = 0f)
    {
        if (textMeshPro != null)
        {
            if (constrainedWidth > 0f)
            {
                return textMeshPro.GetPreferredValues(content, constrainedWidth, 0f);
            }
            return textMeshPro.GetPreferredValues(content);
        }

        return Vector2.zero;
    }
}