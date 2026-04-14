using DG.Tweening;
using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Void = EditorAttributes.Void;

public class HSAbilityUpgradeUI : UIChildNavigate, IBackToLastMenu
{
    [ButtonField("Show", "Show")] public Void ShowHolder;
    [ButtonField("Hide", "Hide")] public Void HideHolder;

    [FoldoutGroup("UI",
        nameof(origin),
        nameof(p1),
        nameof(p2),
        nameof(p3),
        nameof(branch1),
        nameof(branch2),
        nameof(abilityName),
        nameof(lineSprite),
        nameof(mat))]
    public Void uiGroupholder;

    [SerializeField, HideProperty] public Image origin;
    [SerializeField, HideProperty] public Image p1;
    [SerializeField, HideProperty] public Image p2;
    [SerializeField, HideProperty] public Image p3;
    [SerializeField, HideProperty] public Image branch1;
    [SerializeField, HideProperty] public Image branch2;
    [SerializeField, HideProperty] public TextMeshProUGUI abilityName;
    [SerializeField, HideProperty] public Sprite lineSprite;
    public List<GameObject> connectionLines = new List<GameObject>();
    public List<GameObject> upgradedLines = new List<GameObject>();
    public Material mat;
    private bool linesCreated = false;

    public HSEnum abilityEnum;
    [HideInInspector] public IHeartSwordAbility ability;
    [HideInInspector] public HeartSwordAbilities hsManager;

    public Color upgradedColor_transparent = new Color(0.56f, 0.82f, 1, 0f);
    public Color upgradedColor_opaque = new Color(0.56f, 0.82f, 1, 1f);
    private Color normalColor_transparent = new Color(Color.white.r, Color.white.g, Color.white.b, 0f);
    public Color glowColor = new Color(0f, 0.42f, 1, 1f);
    public Color normalColor_opaque = Color.white;

    private void Start()
    {
        if (abilityEnum == HSEnum.None) return;
        upgradedColor_transparent = upgradedColor_opaque;
        upgradedColor_transparent.a = 0f;
        normalColor_transparent = normalColor_opaque;
        normalColor_transparent.a = 0f;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            EventSystemExtension.SetSelectObject(origin.gameObject);
        });

        CreateConnectionLines();
    }

    // Update is called once per frame
    private void Update()
    {
        if (abilityEnum == HSEnum.None) return;
        if (!Application.isPlaying)
        {
            // Only update if lines are created and all references are assigned
            if (linesCreated && connectionLines.Count == 5 &&
                origin != null && p1 != null && p2 != null && p3 != null && branch1 != null && branch2 != null)
            {
                UpdateLinePositions();
            }
        }
    }

    public void ShowOrHide(bool show)
    {
        if (abilityEnum == HSEnum.None) return;
        if (!isActiveAndEnabled) return;
        if (show)
        {
            StartCoroutine(ShowCoroutine());
        }
        else
        {
            StartCoroutine(HideCoroutine());
        }
    }

    public override void OnEnable()
    {
        //if (abilityEnum == HSEnum.None) return;
        if (!Application.isPlaying) hsManager = FindAnyObjectByType<HeartSwordAbilities>();
        else hsManager = HeartSwordAbilities.instance; base.OnEnable();
        if (hsManager == null) return;
        ability = hsManager.GetAbilityByEnum(abilityEnum);
        if (!Application.isPlaying) return;

        List<Image> images = new List<Image> { origin, p1, p2, p3, branch1, branch2 };
        foreach (var lineObj in connectionLines)
        {
            if (lineObj != null)
            {
                var img = lineObj.GetComponent<Image>();
                img.fillAmount = 0f;
                Color _c = img.color;
                _c.a = 1f;
                img.color = _c;
                img.material.SetColor("_GlowColor", glowColor);
            }
        }
        foreach (var lineObj in upgradedLines)
        {
            if (lineObj != null)
            {
                var img = lineObj.GetComponent<Image>();
                img.material.SetColor("_GlowColor", glowColor);
            }
        }
        foreach (var img in images)
        {
            if (img != null)
            {
                Color col = img.color;
                col.a = 0f;
                SetColor(img.material, col);
                img.material.SetColor("_GlowColor", glowColor);

                BackToLastMenu backToMenu;
                if (TryGetComponent<BackToLastMenu>(out BackToLastMenu _backToMenu))
                {
                    backToMenu = _backToMenu;
                }
                else
                {
                    backToMenu = img.gameObject.AddComponent<BackToLastMenu>();
                }
                backToMenu.goBackObject = this.gameObject;
            }
        }
        Color c = abilityName.color;
        c.a = 0f;
        abilityName.color = c;
    }

    public void LevelUp(int index)
    {
        if (ability.GetUpgradedLevel() >= index) return;
        if (!ability.Upgrade(index)) return;
        StartCoroutine(ConnectLinesUICoroutine(index - 1, true));
        //print(ability.GetUpgradedLevel());
    }

    public void UnlockBranch2()
    {
        if (ability.IsBranch2Unlocked()) return;
        if (!ability.UnlockBranch(false)) return;
        StartCoroutine(ConnectLinesUICoroutine(4, true));
        //print("Unlocked branch 2");
    }

    public void UnlockBranch1()
    {
        if (ability.IsBranch1Unlocked()) return;
        if (!ability.UnlockBranch(true)) return;
        StartCoroutine(ConnectLinesUICoroutine(3, true));
        //print("Unlocked branch 1");
    }

    public void UnlockOrigin()
    {
        if (ability.IsUnlocked()) return;
        ability.Unlock();
        SetColor(origin.material, upgradedColor_opaque);
    }

    public IEnumerator ShowCoroutine()
    {
        // Gather all images
        List<Image> images = new List<Image> { p1, p2, p3, branch1, branch2 };
        yield return new WaitForSeconds(0.8f);

        if (ability.IsUnlocked())
        {
            origin.color = upgradedColor_opaque; // Add this line
            SetColor(origin.material, upgradedColor_opaque);
            SetGlowStrength(origin.material, 2f);
        }
        else
        {
            SetColor(origin.material, normalColor_transparent);
            SetGlowStrength(origin.material, 0f);
        }
        StartCoroutine(FadeUI(0.5f, origin, true));

        for (int i = 0; i < images.Count; i++)
        {
            SetColor(images[i].material, normalColor_transparent);

            SetGlowStrength(images[i].material, 0f);
            StartCoroutine(FadeUI(0.5f, images[i], true));
        }

        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(ConnectLinesUICoroutine(0, false));
        yield return StartCoroutine(ConnectLinesUICoroutine(1, false));
        yield return StartCoroutine(ConnectLinesUICoroutine(2, false));
        StartCoroutine(ConnectLinesUICoroutine(3, false));
        StartCoroutine(ConnectLinesUICoroutine(4, false));

        yield return StartCoroutine(ConnectLinesUICoroutine(0, true));
        yield return StartCoroutine(ConnectLinesUICoroutine(1, true));
        yield return StartCoroutine(ConnectLinesUICoroutine(2, true));
        StartCoroutine(ConnectLinesUICoroutine(3, true));
        yield return StartCoroutine(ConnectLinesUICoroutine(4, true));

        yield return StartCoroutine(FadeUI(0.2f, abilityName, true));
    }

    public IEnumerator HideCoroutine()
    {
        StartCoroutine(FadeUI(0.3f, abilityName, false));
        List<Image> images = new List<Image> { origin, p1, p2, p3, branch1, branch2 };
        for (int i = 0; i < images.Count; i++)
        {
            StartCoroutine(FadeUI(0.3f, images[i], false));
        }

        foreach (var lineObj in connectionLines)
        {
            StartCoroutine(FadeUI(0.3f, lineObj.GetComponent<Image>(), false));
        }

        foreach (var lineObj in upgradedLines)
        {
            StartCoroutine(FadeUI(0.3f, lineObj.GetComponent<Image>(), false));
        }

        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < images.Count; i++)
        {
            SetColor(images[i].material, normalColor_transparent);
        }
    }

    public IEnumerator ConnectLinesUICoroutine(int index, bool isUpgraded)
    {
        List<Image> images = new List<Image> { p1, p2, p3, branch1, branch2 };
        Image line = connectionLines[index].GetComponent<Image>();
        if (isUpgraded) line = upgradedLines[index].GetComponent<Image>();
        if (index <= 2) if (isUpgraded && ability.GetUpgradedLevel() <= index) { yield break; }
        if (index == 3) if (isUpgraded && ability.GetUpgradedLevel() <= 3 && !ability.IsBranch1Unlocked()) { yield break; }//branch1
        if (index == 4) if (isUpgraded && ability.GetUpgradedLevel() <= 3 && !ability.IsBranch2Unlocked()) { yield break; }//branch2
        Color col = isUpgraded ? upgradedColor_opaque : normalColor_opaque;
        SetColor(line.material, col);

        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            line.fillAmount = Mathf.Lerp(0, 1f, t);
            SetColor(line.material, col);
            yield return null;
        }

        if (isUpgraded)
        {
            elapsed = 0f;
            duration = 0.2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetColor(images[index].material, Color.Lerp(normalColor_opaque, upgradedColor_opaque, t));
                SetGlowStrength(images[index].material, Mathf.Lerp(0f, 2f, t));
                yield return null;
            }
        }

        yield return null;
    }

    public IEnumerator FadeUI(float duration, Image image, bool fadeIn)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Color c = image.color;
            if (fadeIn) c.a = Mathf.Lerp(0, 1f, t);
            else c.a = Mathf.Lerp(1f, 0, t);
            SetColor(image.material, c);

            yield return null;
        }
    }

    public IEnumerator FadeUI(float duration, TextMeshProUGUI text, bool fadeIn)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Color c = text.color;
            if (fadeIn) c.a = Mathf.Lerp(0, 1f, t);
            else c.a = Mathf.Lerp(1f, 0, t);
            text.color = c;

            yield return null;
        }
    }

    private void CreateConnectionLines()
    {
        // Clear existing lines
        if (origin == null || p1 == null || p2 == null || p3 == null || branch1 == null || branch2 == null)
        {
            Debug.LogWarning("Please assign all Image references before creating connection lines.");
            return;
        }
        foreach (GameObject line in connectionLines) { Destroy(line); }
        connectionLines.Clear();
        foreach (GameObject line in upgradedLines) { Destroy(line); }
        upgradedLines.Clear();

        List<Image> images = new List<Image> { origin, p1, p2, p3, branch1, branch2 };
        for (int i = 0; i < images.Count; i++)
        {
            Material newMat = new Material(mat);
            images[i].material = newMat;
            images[i].material.SetColor("_GlowColor", glowColor);
            SetGlowStrength(images[i].material, 0f);
            SetAlpha(images[i].material, 0);
        }

        CreateLineBetween(origin, p1, false);
        CreateLineBetween(p1, p2, false);
        CreateLineBetween(p2, p3, false);
        CreateLineBetween(p3, branch1, false);
        CreateLineBetween(p3, branch2, false);

        CreateLineBetween(origin, p1, true);
        CreateLineBetween(p1, p2, true);
        CreateLineBetween(p2, p3, true);
        CreateLineBetween(p3, branch1, true);
        CreateLineBetween(p3, branch2, true);
        linesCreated = true;

        origin.transform.SetAsLastSibling();
        p1.transform.SetAsLastSibling();
        p2.transform.SetAsLastSibling();
        p3.transform.SetAsLastSibling();
        branch1.transform.SetAsLastSibling();
        branch2.transform.SetAsLastSibling();
    }

    private void Show()
    {
        ShowOrHide(true);
    }

    private void Hide()
    {
        ShowOrHide(false);
    }

    private void UpdateLinesPosition(Image from, Image to, GameObject lineObj)
    {
        if (from == null || to == null) return;

        RectTransform fromRect = from.rectTransform;
        RectTransform toRect = to.rectTransform;
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        Vector2 start = fromRect.anchoredPosition;
        Vector2 end = toRect.anchoredPosition;
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        lineRect.sizeDelta = new Vector2(2.2f, distance);
        lineRect.pivot = new Vector2(0.5f, 0f);

        // Position at start point (anchoredPosition)
        lineRect.anchoredPosition = start;

        // Rotate to match direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private void UpdateLinePositions()
    {
        UpdateLinesPosition(origin, p1, connectionLines[0]);
        UpdateLinesPosition(p1, p2, connectionLines[1]);
        UpdateLinesPosition(p2, p3, connectionLines[2]);
        UpdateLinesPosition(p3, branch1, connectionLines[3]);
        UpdateLinesPosition(p3, branch2, connectionLines[4]);

        UpdateLinesPosition(origin, p1, upgradedLines[0]);
        UpdateLinesPosition(p1, p2, upgradedLines[1]);
        UpdateLinesPosition(p2, p3, upgradedLines[2]);
        UpdateLinesPosition(p3, branch1, upgradedLines[3]);
        UpdateLinesPosition(p3, branch2, upgradedLines[4]);
    }

    private void CreateLineBetween(Image from, Image to, bool isUpgraded)
    {
        if (from == null || to == null) return;

        GameObject lineObj = new GameObject($"Line_{from.name}_to_{to.name}_{(isUpgraded ? "upgraded" : "")}");
        lineObj.transform.SetParent(transform, false);

        if (isUpgraded) upgradedLines.Add(lineObj);
        else connectionLines.Add(lineObj);

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.sprite = lineSprite;
        lineImage.type = Image.Type.Filled;
        lineImage.fillMethod = Image.FillMethod.Vertical;
        Material newMat = new Material(mat);
        lineObj.GetComponent<Image>().material = newMat;
        lineObj.GetComponent<Image>().material.SetColor("_GlowColor", glowColor);

        if (isUpgraded)
        {
            SetColor(lineImage.material, upgradedColor_opaque);
            SetGlowStrength(lineImage.material, 4f);
            lineImage.fillAmount = 0f;
        }
        else
        {
            lineObj.transform.SetAsFirstSibling();
            SetColor(lineImage.material, normalColor_transparent);
            SetGlowStrength(lineImage.material, 0f);
        }

        RectTransform fromRect = from.rectTransform;
        RectTransform toRect = to.rectTransform;
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        Vector2 start = fromRect.anchoredPosition;
        Vector2 end = toRect.anchoredPosition;
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        lineRect.sizeDelta = new Vector2(2.2f, distance);
        lineRect.pivot = new Vector2(0.5f, 0f);

        // Position at start point (anchoredPosition)
        lineRect.anchoredPosition = start;

        // Rotate to match direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetColor(Material mat, Color color)
    {
        if (mat != null && mat.shader != null && mat.shader.name == "UI/UIGlow")
        {
            mat.SetColor("_Color", color);
        }
    }

    public void SetAlpha(Material mat, float alpha)
    {
        Color col = mat.color;
        col.a = alpha;
        SetColor(mat, col);
    }

    public void SetGlowStrength(Material mat, float strength)
    {
        mat.SetFloat("_GlowStrength", strength);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        eventData.selectedObject.transform.DOComplete();
        Material mat = eventData.selectedObject.GetComponent<Image>().material;
        DOTween.To(
            () => mat.GetFloat("_GlowStrength"),
            x => SetGlowStrength(mat, x),
            8f,
            0.3f
        ).SetEase(Ease.OutQuad);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        eventData.selectedObject.transform.DOComplete();
        Material mat = eventData.selectedObject.GetComponent<Image>().material;

        float glowStrength = 0f;
        if (eventData.selectedObject == origin.gameObject)
        {
            if (ability.IsUnlocked()) { glowStrength = 2f; }
        }
        else
        {
            List<Image> images = new List<Image> { p1, p2, p3, branch1, branch2 };
            int index = images.IndexOf(eventData.selectedObject.GetComponent<Image>());
            if (index <= 2) if (ability.GetUpgradedLevel() > index) { glowStrength = 2f; }
            if (index == 3) if (ability.GetUpgradedLevel() >= 3 && ability.IsBranch1Unlocked()) { glowStrength = 2f; }//branch1
            if (index == 4) if (ability.GetUpgradedLevel() >= 3 && ability.IsBranch2Unlocked()) { glowStrength = 2f; }//branch2
        }

        DOTween.To(
            () => mat.GetFloat("_GlowStrength"),
            x => SetGlowStrength(mat, x),
            glowStrength,
            0.3f
        ).SetEase(Ease.OutQuad);
    }

    public void GoBack()
    {
        MenuManager.instance.CloseHSUpgradeMenu(true);
    }
}