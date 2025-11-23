using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EditorAttributes;
using TMPro;

[ExecuteAlways]
public class HSAbilityUpgradeUI : MonoBehaviour
{
    [ButtonField("CreateConnectionLines", "CreateConnectionLines")] public Void buttonHolder;
    public Image origin;
    public Image p1;
    public Image p2;
    public Image p3;
    public Image branch1;
    public Image branch2;
    public TextMeshProUGUI abilityName;
    public List<GameObject> connectionLines = new List<GameObject>();
    private bool linesCreated = false;
    public Sprite lineSprite;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Application.isPlaying)
        {
            // Only update if lines are created and all references are assigned
            if (linesCreated && connectionLines.Count == 5 &&
                origin != null && p1 != null && p2 != null && p3 != null && branch1 != null && branch2 != null)
            {
                UpdateConnectLines();
            }
        }
    }

    public void ShowOrHide(bool show)
    {
        if (show)
        {
            StartCoroutine(ShowCoroutine());
        }
        else
        {
            StartCoroutine(HideCoroutine());
        }
    }

    private void OnEnable()
    {
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
            }
        }
        foreach (var img in images)
        {
            if (img != null)
            {
                Color col = img.color;
                col.a = 0f;
                img.color = col;
            }
        }
        Color c = abilityName.color;
        c.a = 0f;
        abilityName.color = c;
    }

    public IEnumerator ShowCoroutine()
    {
        // Gather all images
        List<Image> images = new List<Image> { origin, p1, p2, p3, branch1, branch2 };
        yield return new WaitForSeconds(0.8f);

        for (int i = 0; i < images.Count; i++)
        {
            StartCoroutine(FadeUI(0.5f, images[i], true));
        }
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(ConnectLinesUICoroutine(connectionLines[0].GetComponent<Image>()));
        yield return StartCoroutine(ConnectLinesUICoroutine(connectionLines[1].GetComponent<Image>()));
        yield return StartCoroutine(ConnectLinesUICoroutine(connectionLines[2].GetComponent<Image>()));
        StartCoroutine(ConnectLinesUICoroutine(connectionLines[3].GetComponent<Image>()));
        yield return StartCoroutine(ConnectLinesUICoroutine(connectionLines[4].GetComponent<Image>()));

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

        yield return new WaitForSeconds(0.3f);
    }

    public IEnumerator ConnectLinesUICoroutine(Image line)
    {
        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            line.fillAmount = Mathf.Lerp(0, 1f, t);
            yield return null;
        }
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
            image.color = c;

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
        foreach (GameObject line in connectionLines) { DestroyImmediate(line); }
        connectionLines.Clear();
        CreateLineBetween(origin, p1);
        CreateLineBetween(p1, p2);
        CreateLineBetween(p2, p3);
        CreateLineBetween(p3, branch1);
        CreateLineBetween(p3, branch2);
        linesCreated = true;
    }

    private void UpdateConnectLinesBetween(Image from, Image to, GameObject lineObj)
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

    private void UpdateConnectLines()
    {
        UpdateConnectLinesBetween(origin, p1, connectionLines[0]);
        UpdateConnectLinesBetween(p1, p2, connectionLines[1]);
        UpdateConnectLinesBetween(p2, p3, connectionLines[2]);
        UpdateConnectLinesBetween(p3, branch1, connectionLines[3]);
        UpdateConnectLinesBetween(p3, branch2, connectionLines[4]);
    }

    private void CreateLineBetween(Image from, Image to)
    {
        if (from == null || to == null) return;

        GameObject lineObj = new GameObject($"Line_{from.name}_to_{to.name}");
        lineObj.transform.SetParent(transform, false);
        connectionLines.Add(lineObj);

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.sprite = lineSprite;
        lineImage.type = Image.Type.Filled;
        lineImage.fillMethod = Image.FillMethod.Vertical;
        lineImage.color = Color.white;

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
}