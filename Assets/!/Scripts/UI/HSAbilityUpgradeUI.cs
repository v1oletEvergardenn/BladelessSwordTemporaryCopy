using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EditorAttributes;

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

    public List<GameObject> connectionLines = new List<GameObject>();
    private bool linesCreated = false;

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