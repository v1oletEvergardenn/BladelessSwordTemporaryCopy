using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GeneralChildNavigateUI : UIChildNavigate
{
    [Header("Effect Settings")]
    [SerializeField] private List<UISelectionEffect> _effects = new List<UISelectionEffect>();

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.15f;

    [SerializeField] private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Dictionary<GameObject, Coroutine> _activeAnimations = new Dictionary<GameObject, Coroutine>();
    private Dictionary<GameObject, EffectState> _originalStates = new Dictionary<GameObject, EffectState>();

    public override void OnSelect(BaseEventData eventData)
    {
        GameObject target = eventData.selectedObject;
        if (target == null) return;

        _lastSelected = target.GetComponent<Selectable>();

        StopActiveAnimation(target);
        CacheOriginalState(target);

        _activeAnimations[target] = StartCoroutine(AnimateEffects(target, true));
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        GameObject target = eventData.selectedObject;
        if (target == null) return;

        StopActiveAnimation(target);

        _activeAnimations[target] = StartCoroutine(AnimateEffects(target, false));
    }

    private void CacheOriginalState(GameObject target)
    {
        if (_originalStates.ContainsKey(target)) return;

        var state = new EffectState();
        state.originalScale = target.transform.localScale;
        state.originalPosition = target.transform.localPosition;
        state.originalRotation = target.transform.localRotation;

        var graphic = target.GetComponent<Graphic>();
        if (graphic != null)
            state.originalColor = graphic.color;

        var image = target.GetComponent<Image>();
        if (image != null)
            state.originalSprite = image.sprite;

        var canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            state.originalAlpha = canvasGroup.alpha;

        var rectTransform = target.GetComponent<RectTransform>();
        if (rectTransform != null)
            state.originalAnchoredPosition = rectTransform.anchoredPosition;

        _originalStates[target] = state;
    }

    private void StopActiveAnimation(GameObject target)
    {
        if (_activeAnimations.TryGetValue(target, out Coroutine coroutine) && coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    private IEnumerator AnimateEffects(GameObject target, bool isSelected)
    {
        if (!_originalStates.TryGetValue(target, out EffectState originalState))
            yield break;

        float elapsed = 0f;

        while (elapsed < _animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = _easeCurve.Evaluate(Mathf.Clamp01(elapsed / _animationDuration));

            foreach (var effect in _effects)
            {
                if (!effect.enabled) continue;
                ApplyEffect(target, effect, originalState, t, isSelected);
            }

            yield return null;
        }

        // Ensure final state
        foreach (var effect in _effects)
        {
            if (!effect.enabled) continue;
            ApplyEffect(target, effect, originalState, 1f, isSelected);
        }
    }

    private void ApplyEffect(GameObject target, UISelectionEffect effect, EffectState originalState, float t, bool isSelected)
    {
        switch (effect.UIeffectType)
        {
            case UIEffectType.Scale:
                ApplyScaleEffect(target, effect, originalState, t, isSelected);
                break;

            case UIEffectType.Color:
                ApplyColorEffect(target, effect, originalState, t, isSelected);
                break;

            case UIEffectType.Move:
                ApplyMoveEffect(target, effect, originalState, t, isSelected);
                break;

            case UIEffectType.Sprite:
                ApplySpriteEffect(target, effect, originalState, isSelected);
                break;

            case UIEffectType.Rotate:
                ApplyRotateEffect(target, effect, originalState, t, isSelected);
                break;

            case UIEffectType.Fade:
                ApplyFadeEffect(target, effect, originalState, t, isSelected);
                break;

            case UIEffectType.Punch:
                // Punch is instant, handled separately
                break;

            case UIEffectType.Shake:
                // Shake is handled in a separate coroutine
                break;

            case UIEffectType.Outline:
                ApplyOutlineEffect(target, effect, isSelected);
                break;

            case UIEffectType.Shadow:
                ApplyShadowEffect(target, effect, isSelected);
                break;
        }
    }

    #region Effect Implementations

    private void ApplyScaleEffect(GameObject target, UISelectionEffect effect, EffectState state, float t, bool isSelected)
    {
        Vector3 targetScale = isSelected ? state.originalScale * effect.scaleMultiplier : state.originalScale;
        Vector3 startScale = isSelected ? state.originalScale : state.originalScale * effect.scaleMultiplier;
        target.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
    }

    private void ApplyColorEffect(GameObject target, UISelectionEffect effect, EffectState state, float t, bool isSelected)
    {
        var graphic = target.GetComponent<Graphic>();
        if (graphic == null) return;

        Color targetColor = isSelected ? effect.selectedColor : state.originalColor;
        Color startColor = isSelected ? state.originalColor : effect.selectedColor;
        graphic.color = Color.Lerp(startColor, targetColor, t);
    }

    private void ApplyMoveEffect(GameObject target, UISelectionEffect effect, EffectState state, float t, bool isSelected)
    {
        var rectTransform = target.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Vector2 targetPos = isSelected ? state.originalAnchoredPosition + effect.moveOffset : state.originalAnchoredPosition;
            Vector2 startPos = isSelected ? state.originalAnchoredPosition : state.originalAnchoredPosition + effect.moveOffset;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
        }
        else
        {
            Vector3 targetPos = isSelected ? state.originalPosition + (Vector3)effect.moveOffset : state.originalPosition;
            Vector3 startPos = isSelected ? state.originalPosition : state.originalPosition + (Vector3)effect.moveOffset;
            target.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
        }
    }

    private void ApplySpriteEffect(GameObject target, UISelectionEffect effect, EffectState state, bool isSelected)
    {
        var image = target.GetComponent<Image>();
        if (image == null) return;

        image.sprite = isSelected ? effect.selectedSprite : state.originalSprite;
    }

    private void ApplyRotateEffect(GameObject target, UISelectionEffect effect, EffectState state, float t, bool isSelected)
    {
        Quaternion targetRot = isSelected
            ? state.originalRotation * Quaternion.Euler(effect.rotationAngle)
            : state.originalRotation;
        Quaternion startRot = isSelected
            ? state.originalRotation
            : state.originalRotation * Quaternion.Euler(effect.rotationAngle);
        target.transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);
    }

    private void ApplyFadeEffect(GameObject target, UISelectionEffect effect, EffectState state, float t, bool isSelected)
    {
        var canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
            state.originalAlpha = 1f;
        }

        float targetAlpha = isSelected ? effect.selectedAlpha : state.originalAlpha;
        float startAlpha = isSelected ? state.originalAlpha : effect.selectedAlpha;
        canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
    }

    private void ApplyOutlineEffect(GameObject target, UISelectionEffect effect, bool isSelected)
    {
        var outline = target.GetComponent<Outline>();
        if (outline == null && isSelected)
        {
            outline = target.AddComponent<Outline>();
        }

        if (outline != null)
        {
            outline.enabled = isSelected;
            if (isSelected)
            {
                outline.effectColor = effect.outlineColor;
                outline.effectDistance = effect.outlineDistance;
            }
        }
    }

    private void ApplyShadowEffect(GameObject target, UISelectionEffect effect, bool isSelected)
    {
        var shadow = target.GetComponent<Shadow>();
        if (shadow == null && isSelected)
        {
            shadow = target.AddComponent<Shadow>();
        }

        if (shadow != null)
        {
            shadow.enabled = isSelected;
            if (isSelected)
            {
                shadow.effectColor = effect.shadowColor;
                shadow.effectDistance = effect.shadowDistance;
            }
        }
    }

    #endregion Effect Implementations

    #region Special Effects (Call these manually if needed)

    public void TriggerPunchEffect(GameObject target, Vector3 punchScale, float duration)
    {
        StartCoroutine(PunchScaleCoroutine(target, punchScale, duration));
    }

    private IEnumerator PunchScaleCoroutine(GameObject target, Vector3 punchScale, float duration)
    {
        Vector3 originalScale = target.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Punch effect: quick scale up then back
            float punch = Mathf.Sin(t * Mathf.PI) * (1 - t);
            target.transform.localScale = originalScale + punchScale * punch;
            yield return null;
        }

        target.transform.localScale = originalScale;
    }

    public void TriggerShakeEffect(GameObject target, float intensity, float duration)
    {
        StartCoroutine(ShakeCoroutine(target, intensity, duration));
    }

    private IEnumerator ShakeCoroutine(GameObject target, float intensity, float duration)
    {
        Vector3 originalPos = target.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float dampening = 1 - (elapsed / duration);
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * intensity * dampening,
                UnityEngine.Random.Range(-1f, 1f) * intensity * dampening,
                0
            );
            target.transform.localPosition = originalPos + offset;
            yield return null;
        }

        target.transform.localPosition = originalPos;
    }

    #endregion Special Effects (Call these manually if needed)

    private void Start()
    {
    }

    private void Update()
    {
    }
}

[Serializable]
public class UISelectionEffect
{
    [HorizontalGroup("Header"), LabelWidth(50)]
    public bool enabled = true;

    [HorizontalGroup("Header"), LabelWidth(80)]
    public UIEffectType UIeffectType;

    // Scale Settings
    [ShowIf("UIeffectType", UIEffectType.Scale)]
    [Tooltip("Multiplier applied to original scale when selected")]
    public float scaleMultiplier = 1.1f;

    // Color Settings
    [ShowIf("UIeffectType", UIEffectType.Color)]
    public Color selectedColor = Color.white;

    // Move Settings
    [ShowIf("UIeffectType", UIEffectType.Move)]
    [Tooltip("Offset from original position when selected")]
    public Vector2 moveOffset = new Vector2(10f, 0f);

    // Sprite Settings
    [ShowIf("UIeffectType", UIEffectType.Sprite)]
    [PreviewField(50, ObjectFieldAlignment.Left)]
    public Sprite selectedSprite;

    // Rotation Settings
    [ShowIf("UIeffectType", UIEffectType.Rotate)]
    [Tooltip("Euler angles to rotate when selected")]
    public Vector3 rotationAngle = new Vector3(0, 0, 5f);

    // Fade Settings
    [ShowIf("UIeffectType", UIEffectType.Fade)]
    [Range(0f, 1f)]
    public float selectedAlpha = 1f;

    // Outline Settings
    [ShowIf("UIeffectType", UIEffectType.Outline)]
    public Color outlineColor = Color.yellow;

    [ShowIf("UIeffectType", UIEffectType.Outline)]
    public Vector2 outlineDistance = new Vector2(2f, 2f);

    // Shadow Settings
    [ShowIf("UIeffectType", UIEffectType.Shadow)]
    public Color shadowColor = new Color(0, 0, 0, 0.5f);

    [ShowIf("UIeffectType", UIEffectType.Shadow)]
    public Vector2 shadowDistance = new Vector2(3f, -3f);

    // Info boxes for manual trigger effects
    [ShowIf("UIeffectType", UIEffectType.Punch)]
    [InfoBox("Use TriggerPunchEffect() to trigger this effect manually.", InfoMessageType.Info)]
    [ShowIf("UIeffectType", UIEffectType.Punch)]
    public float punchIntensity = 0.2f;

    [ShowIf("UIeffectType", UIEffectType.Punch)]
    public float punchDuration = 0.3f;

    [ShowIf("UIeffectType", UIEffectType.Shake)]
    [InfoBox("Use TriggerShakeEffect() to trigger this effect manually.", InfoMessageType.Info)]
    [ShowIf("UIeffectType", UIEffectType.Shake)]
    public float shakeIntensity = 5f;

    [ShowIf("UIeffectType", UIEffectType.Shake)]
    public float shakeDuration = 0.3f;
}

public enum UIEffectType
{
    Scale,      // Scale up/down
    Color,      // Change color
    Move,       // Move in a direction
    Sprite,     // Swap sprite
    Rotate,     // Rotate element
    Fade,       // Change alpha/opacity
    Punch,      // Quick punch scale effect
    Shake,      // Shake effect
    Outline,    // Add/remove outline
    Shadow      // Add/remove shadow
}

public class EffectState
{
    public Vector3 originalScale;
    public Vector3 originalPosition;
    public Vector2 originalAnchoredPosition;
    public Quaternion originalRotation;
    public Color originalColor;
    public Sprite originalSprite;
    public float originalAlpha;
}