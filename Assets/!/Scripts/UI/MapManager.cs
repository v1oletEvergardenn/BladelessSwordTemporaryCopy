using DG.Tweening;
using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum MapIconType
{
    Player,
    Shop,
    Quest,
    SavePoint,
    Custom1,
    Custom2,
    Custom3,
    Custom4
}

public class MapManager : MonoBehaviour
{
    #region Singleton

    public static MapManager instance;

    private void Awake()
    {
        if (instance == null) { instance = this; }
    }

    #endregion Singleton

    #region References

    [SerializeField] private GameObject mapCanvas;
    [SerializeField] private RectTransform mapContainer;

    [SerializeField] private RectTransform cursorIcon;
    [SerializeField] private float cursorMoveSpeed = 300f;
    [SerializeField] private float cursorSnapDistance = 50f;
    [SerializeField] private float cursorSmoothSpeed = 15f;

    [SerializeField] private Transform iconsContainer;
    [SerializeField] private GameObject iconsSelector;

    [SerializeField] private Vector2 worldPointBotLeft;
    [SerializeField] private Vector2 worldPointTopRight;
    [SerializeField] private RectTransform mapPointBotLeft;
    [SerializeField] private RectTransform mapPointTopRight;

    // Zoom settings
    [Header("Zoom Settings")]
    [SerializeField] private float minZoom = 0.5f;

    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float smoothSpeed = 12f;
    private float currentZoom = 1f;
    private float targetZoom = 1f;
    private Vector2 targetPanPosition = Vector2.zero;
    private bool isZoomingIn = false;
    private bool isZoomingOut = false;

    // Store the map point under cursor for consistent zoom pivot
    private Vector2 zoomPivotMapPoint = Vector2.zero;

    // Edge panning settings
    [Header("Edge Panning Settings")]
    [SerializeField] private float edgePanThreshold = 80f;

    [SerializeField] private float edgePanSpeed = 200f;

    // Auto-calculated conversion values
    private Vector2 scale;

    private Vector2 offset;

    // Map boundary reference
    private RectTransform mapCanvasRect;

    private InputMaster inputMaster;
    private List<MapIcon> placedCustomIcons = new List<MapIcon>();
    private List<MapIcon> placedWorldIcons = new List<MapIcon>();
    private MapIcon selectedIcon;
    private bool isMapOpen = false;
    private Vector2 cursorInput;
    private Vector2 targetCursorPosition;
    private bool isSelectingIcon = false;

    public Sprite playerIconSprite;
    public Sprite shopIconSprite;
    public Sprite questIconSprite;
    public Sprite savePointIconSprite;
    public Sprite customIconSprite1;
    public Sprite customIconSprite2;
    public Sprite customIconSprite3;
    public Sprite customIconSprite4;

    #endregion References

    #region Unity Lifecycle

    private void Start()
    {
        inputMaster = InputMaster.instance;

        inputMaster._OpenMapAction.performed += ctx => OpenMap();
        inputMaster._CloseMapAction.performed += ctx => CloseMap();
        inputMaster.uiActions.Move.performed += ctx => cursorInput = ctx.ReadValue<Vector2>();
        inputMaster.uiActions.Move.canceled += ctx => cursorInput = Vector2.zero;
        inputMaster.uiActions.Submit.started += ctx => OnPlaceOrSelectIcon();
        inputMaster.uiActions.Delete.performed += ctx => OnRemoveSelectedIcon();
        inputMaster.uiActions.Cancel.performed += ctx => CancelPlaceOrSelection();

        // Continuous zoom - starts when pressed, stops when released
        inputMaster.uiActions.ZoomIn.started += ctx => OnZoomStart(true);
        inputMaster.uiActions.ZoomIn.canceled += ctx => isZoomingIn = false;
        inputMaster.uiActions.ZoomOut.started += ctx => OnZoomStart(false);
        inputMaster.uiActions.ZoomOut.canceled += ctx => isZoomingOut = false;

        mapCanvas.SetActive(false);
        mapCanvasRect = mapCanvas.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!isMapOpen) return;

        HandleZoomInput();
        UpdateZoomAndPan();
        UpdateEdgePanning();
        UpdateAllIconPosition();
        UpdateCursorMovement();
        CheckForNearbyIcons();
    }

    private void OnDestroy()
    {
        if (inputMaster != null)
        {
            inputMaster._OpenMapAction.performed -= ctx => OpenMap();
            inputMaster._CloseMapAction.performed -= ctx => CloseMap();
            inputMaster.uiActions.Move.performed -= ctx => cursorInput = ctx.ReadValue<Vector2>();
            inputMaster.uiActions.Move.canceled -= ctx => cursorInput = Vector2.zero;
            inputMaster.uiActions.Submit.started -= ctx => OnPlaceOrSelectIcon();
            inputMaster.uiActions.Delete.performed -= ctx => OnRemoveSelectedIcon();
            inputMaster.uiActions.Cancel.performed -= ctx => CancelPlaceOrSelection();
            inputMaster.uiActions.ZoomIn.started -= ctx => OnZoomStart(true);
            inputMaster.uiActions.ZoomIn.canceled -= ctx => isZoomingIn = false;
            inputMaster.uiActions.ZoomOut.started -= ctx => OnZoomStart(false);
            inputMaster.uiActions.ZoomOut.canceled -= ctx => isZoomingOut = false;
        }
    }

    #endregion Unity Lifecycle

    #region Zoom

    private void OnZoomStart(bool zoomIn)
    {
        if (zoomIn)
            isZoomingIn = true;
        else
            isZoomingOut = true;

        // Capture the map point under the cursor at the start of zoom
        // This point should stay under the cursor throughout the zoom operation
        zoomPivotMapPoint = ScreenPointToMapPoint(cursorIcon.anchoredPosition);
    }

    /// <summary>
    /// Converts a screen position to the corresponding point on the unscaled map
    /// </summary>
    private Vector2 ScreenPointToMapPoint(Vector2 screenPos)
    {
        // screenPos = mapPoint * zoom + panOffset
        // mapPoint = (screenPos - panOffset) / zoom
        return (screenPos - targetPanPosition) / targetZoom;
    }

    /// <summary>
    /// Converts a map point to screen position at a given zoom and pan
    /// </summary>
    private Vector2 MapPointToScreenPoint(Vector2 mapPoint, float zoom, Vector2 panOffset)
    {
        return mapPoint * zoom + panOffset;
    }

    private void HandleZoomInput()
    {
        if (!isZoomingIn && !isZoomingOut) return;

        float previousTargetZoom = targetZoom;

        if (isZoomingIn)
        {
            targetZoom = Mathf.Clamp(targetZoom + zoomSpeed * Time.unscaledDeltaTime, minZoom, maxZoom);
        }
        else if (isZoomingOut)
        {
            targetZoom = Mathf.Clamp(targetZoom - zoomSpeed * Time.unscaledDeltaTime, minZoom, maxZoom);
        }

        // Only recalculate target pan if zoom target changed
        if (!Mathf.Approximately(previousTargetZoom, targetZoom))
        {
            // Calculate pan position that keeps the pivot map point under the cursor
            // cursorPos = pivotMapPoint * targetZoom + targetPanPosition
            // targetPanPosition = cursorPos - pivotMapPoint * targetZoom
            targetPanPosition = cursorIcon.anchoredPosition - zoomPivotMapPoint * targetZoom;

            // Clamp target pan position for the target zoom level
            targetPanPosition = CalculateClampedPosition(targetPanPosition, targetZoom);
        }
    }

    private void UpdateZoomAndPan()
    {
        bool zoomNeedsUpdate = !Mathf.Approximately(currentZoom, targetZoom);
        bool panNeedsUpdate = Vector2.Distance(mapContainer.anchoredPosition, targetPanPosition) > 0.01f;

        if (!zoomNeedsUpdate && !panNeedsUpdate) return;

        // Use the same lerp factor for both zoom and pan so they move together
        float lerpFactor = Time.unscaledDeltaTime * smoothSpeed;

        // Lerp zoom
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, lerpFactor);
        if (Mathf.Abs(currentZoom - targetZoom) < 0.001f)
            currentZoom = targetZoom;

        // Lerp pan with the same factor
        Vector2 newPanPosition = Vector2.Lerp(mapContainer.anchoredPosition, targetPanPosition, lerpFactor);
        if (Vector2.Distance(newPanPosition, targetPanPosition) < 0.1f)
            newPanPosition = targetPanPosition;

        // Apply both together
        mapContainer.localScale = Vector3.one * currentZoom;
        mapContainer.anchoredPosition = newPanPosition;
    }

    private Vector2 CalculateClampedPosition(Vector2 position, float zoom)
    {
        if (zoom <= 1f)
        {
            return Vector2.zero;
        }

        Rect mapRect = mapContainer.rect;
        float scaledHalfWidth = mapRect.width * zoom / 2f;
        float scaledHalfHeight = mapRect.height * zoom / 2f;
        float viewHalfWidth = mapRect.width / 2f;
        float viewHalfHeight = mapRect.height / 2f;

        float maxOffsetX = scaledHalfWidth - viewHalfWidth;
        float maxOffsetY = scaledHalfHeight - viewHalfHeight;

        Vector2 clampedPos = position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, -maxOffsetX, maxOffsetX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, -maxOffsetY, maxOffsetY);
        return clampedPos;
    }

    private void ResetZoom()
    {
        currentZoom = 1f;
        targetZoom = 1f;
        isZoomingIn = false;
        isZoomingOut = false;
        mapContainer.localScale = Vector3.one;
        mapContainer.anchoredPosition = Vector2.zero;
        targetPanPosition = Vector2.zero;
        zoomPivotMapPoint = Vector2.zero;
    }

    #endregion Zoom

    #region Edge Panning

    private void UpdateEdgePanning()
    {
        if (targetZoom <= 1f || isSelectingIcon) return;

        // Only pan when there's active cursor input
        if (cursorInput.sqrMagnitude < 0.001f) return;

        // Get the visible map area bounds (in screen/canvas space)
        Rect mapRect = mapContainer.rect;
        float viewHalfWidth = mapRect.width / 2f;
        float viewHalfHeight = mapRect.height / 2f;

        Vector2 cursorPos = cursorIcon.anchoredPosition;
        Vector2 panDirection = Vector2.zero;

        // Check if cursor is near edges and calculate pan direction
        if (cursorPos.x > viewHalfWidth - edgePanThreshold)
        {
            float edgeFactor = Mathf.InverseLerp(viewHalfWidth - edgePanThreshold, viewHalfWidth, cursorPos.x);
            panDirection.x = -edgeFactor;
        }
        else if (cursorPos.x < -viewHalfWidth + edgePanThreshold)
        {
            float edgeFactor = Mathf.InverseLerp(-viewHalfWidth + edgePanThreshold, -viewHalfWidth, cursorPos.x);
            panDirection.x = edgeFactor;
        }

        if (cursorPos.y > viewHalfHeight - edgePanThreshold)
        {
            float edgeFactor = Mathf.InverseLerp(viewHalfHeight - edgePanThreshold, viewHalfHeight, cursorPos.y);
            panDirection.y = -edgeFactor;
        }
        else if (cursorPos.y < -viewHalfHeight + edgePanThreshold)
        {
            float edgeFactor = Mathf.InverseLerp(-viewHalfHeight + edgePanThreshold, -viewHalfHeight, cursorPos.y);
            panDirection.y = edgeFactor;
        }

        if (panDirection.sqrMagnitude > 0.001f)
        {
            targetPanPosition += panDirection * edgePanSpeed * Time.unscaledDeltaTime;
            targetPanPosition = CalculateClampedPosition(targetPanPosition, targetZoom);
        }
    }

    #endregion Edge Panning

    #region Map Open/Close

    public void OpenMap()
    {
        if (isMapOpen) return;

        isMapOpen = true;
        mapCanvas.SetActive(true);

        ResetZoom();

        bool foundPlayer = false;
        //try to find player icon and set cursor to it.

        UpdateAllIconPosition();
        foreach (var icon in placedWorldIcons)
        {
            if (icon.iconType == MapIconType.Player)
            {
                targetCursorPosition = icon.RectTransform.anchoredPosition;
                cursorIcon.anchoredPosition = targetCursorPosition;
                foundPlayer = true;
                break;
            }
        }
        if (!foundPlayer)
        {
            targetCursorPosition = Vector2.zero;
            cursorIcon.anchoredPosition = Vector2.zero;
        }
        // If player icon doesn't exist, place cursor in the middle of the map

        InputMaster.SwitchToUIAction();
        GameManager.instance.PauseGame();
        DeselectIcon();

        mapCanvas.GetComponent<CanvasGroup>()?.DOFade(1f, 0.2f).From(0f).SetUpdate(true);
    }

    public void CloseMap()
    {
        if (!isMapOpen) return;

        isMapOpen = false;
        isZoomingIn = false;
        isZoomingOut = false;
        DeselectIcon();
        InputMaster.SwitchToGameplayAction();
        GameManager.instance.UnpauseGame();

        var canvasGroup = mapCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => mapCanvas.SetActive(false));
        }
        else
        {
            mapCanvas.SetActive(false);
        }
    }

    #endregion Map Open/Close

    #region Icon

    public void UpdateAllIconPosition()
    {
        foreach (var icon in placedWorldIcons)
        {
            UpdateWorldIconPosition(icon);
        }
        foreach (var icon in placedCustomIcons)
        {
            UpdateCustomIconPosition(icon);
        }
    }

    private void UpdateWorldIconPosition(MapIcon mapIcon)
    {
        Vector3 Pos = mapIcon.worldGameobject.transform.position;

        float normalizedX = Mathf.InverseLerp(worldPointBotLeft.x, worldPointTopRight.x, Pos.x);
        float normalizedY = Mathf.InverseLerp(worldPointBotLeft.y, worldPointTopRight.y, Pos.y);

        // Calculate base position on map (unscaled)
        float baseX = Mathf.Lerp(mapPointBotLeft.localPosition.x, mapPointTopRight.localPosition.x, normalizedX);
        float baseY = Mathf.Lerp(mapPointBotLeft.localPosition.y, mapPointTopRight.localPosition.y, normalizedY);

        // Apply zoom scale and container offset
        float uiX = baseX * currentZoom + mapContainer.anchoredPosition.x;
        float uiY = baseY * currentZoom + mapContainer.anchoredPosition.y;

        mapIcon.RectTransform.anchoredPosition = new Vector2(uiX, uiY);
    }

    public void CreateMapIcon(MapIconType iconType, GameObject worldGameobject)
    {
        if (CheckDuplicatedMapIcons(worldGameobject)) return;

        GameObject iconObj = new GameObject("MapIcon", typeof(RectTransform));
        iconObj.transform.SetParent(iconsContainer, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconObj.AddComponent<Image>().color = Color.white;
        iconObj.GetComponent<Image>().sprite = GetIconByType(iconType);
        iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30f);
        iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);
        MapIcon mapIcon = iconObj.AddComponent<MapIcon>();
        mapIcon.iconType = iconType;
        mapIcon.worldGameobject = worldGameobject;

        // Store the unscaled, unoffset position as original
        Vector2 originalPos = (iconRect.anchoredPosition - mapContainer.anchoredPosition) / currentZoom;
        mapIcon.Initialize(iconRect, originalPos);
        placedWorldIcons.Add(mapIcon);

        iconRect.localScale = Vector3.zero;
        iconRect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);

        isSelectingIcon = false;
    }

    public bool CheckDuplicatedMapIcons(GameObject iconToCheck)
    {
        foreach (var icon in placedWorldIcons)
        {
            if (icon.worldGameobject == iconToCheck) return true;
        }
        return false;
    }

    public void UpdateAllCustomIconPosition()
    {
        foreach (var icon in placedCustomIcons)
        {
            UpdateCustomIconPosition(icon);
        }
    }

    private void UpdateCustomIconPosition(MapIcon mapIcon)
    {
        // Apply zoom scale and container offset to the original unscaled position
        float uiX = mapIcon.originalPosition.x * currentZoom + mapContainer.anchoredPosition.x;
        float uiY = mapIcon.originalPosition.y * currentZoom + mapContainer.anchoredPosition.y;
        mapIcon.RectTransform.anchoredPosition = new Vector2(uiX, uiY);
    }

    #endregion Icon

    #region Cursor Movement

    private void UpdateCursorMovement()
    {
        if (cursorIcon == null || isSelectingIcon) return;

        if (selectedIcon != null)
        {
            targetCursorPosition = selectedIcon.RectTransform.anchoredPosition;
            cursorIcon.anchoredPosition = Vector2.Lerp(
                cursorIcon.anchoredPosition,
                targetCursorPosition,
                Time.unscaledDeltaTime * cursorSmoothSpeed
            );
            return;
        }

        // Update target position based on input
        Vector2 movement = cursorInput * cursorMoveSpeed * Time.unscaledDeltaTime;
        targetCursorPosition += movement;

        // Clamp target cursor to visible map area (viewport bounds)
        if (mapContainer != null)
        {
            Rect mapRect = mapContainer.rect;
            float viewHalfWidth = mapRect.width / 2f;
            float viewHalfHeight = mapRect.height / 2f;

            targetCursorPosition.x = Mathf.Clamp(targetCursorPosition.x, -viewHalfWidth, viewHalfWidth);
            targetCursorPosition.y = Mathf.Clamp(targetCursorPosition.y, -viewHalfHeight, viewHalfHeight);
        }

        // Smoothly interpolate cursor position
        cursorIcon.anchoredPosition = Vector2.Lerp(
            cursorIcon.anchoredPosition,
            targetCursorPosition,
            Time.unscaledDeltaTime * cursorSmoothSpeed
        );

        // Snap if very close
        if (Vector2.Distance(cursorIcon.anchoredPosition, targetCursorPosition) < 0.5f)
            cursorIcon.anchoredPosition = targetCursorPosition;
    }

    #endregion Cursor Movement

    #region Icon Placement

    private void OnPlaceOrSelectIcon()
    {
        if (!isMapOpen) return;

        if (selectedIcon != null)
        {
            DeselectIcon();
            return;
        }
        isSelectingIcon = true;
        MapIcon nearbyIcon = GetNearestIcon(cursorIcon.anchoredPosition, cursorSnapDistance);
        if (nearbyIcon != null)
        {
            SelectIcon(nearbyIcon);
            return;
        }

        iconsSelector.GetComponent<RectTransform>().anchoredPosition = cursorIcon.anchoredPosition;
        iconsSelector.SetActive(true);
    }

    private void CancelPlaceOrSelection()
    {
        iconsSelector.SetActive(false);
        isSelectingIcon = false;
        if (selectedIcon != null)
        {
            DeselectIcon();
        }
    }

    public void SelectIcon(Image img)
    {
        CreateIconAtCursor(img.sprite);
        iconsSelector.SetActive(false);
    }

    private void CreateIconAtCursor(Sprite img)
    {
        if (iconsContainer == null) return;

        GameObject iconObj = new GameObject("MapIcon", typeof(RectTransform));
        iconObj.transform.SetParent(iconsContainer, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchoredPosition = cursorIcon.anchoredPosition;
        iconObj.AddComponent<Image>().color = Color.white;
        iconObj.GetComponent<Image>().sprite = img;
        iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30f);
        iconRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 30f);
        MapIcon mapIcon = iconObj.AddComponent<MapIcon>();

        // Store the unscaled, unoffset position as original
        Vector2 originalPos = (iconRect.anchoredPosition - mapContainer.anchoredPosition) / currentZoom;
        mapIcon.Initialize(iconRect, originalPos);
        placedCustomIcons.Add(mapIcon);

        iconRect.localScale = Vector3.zero;
        iconRect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);

        isSelectingIcon = false;
    }

    private void OnRemoveSelectedIcon()
    {
        if (!isMapOpen || selectedIcon == null) return;
        if (placedWorldIcons.Contains(selectedIcon)) return;
        RemoveIcon(selectedIcon);
    }

    private void RemoveIcon(MapIcon icon)
    {
        if (icon == null) return;

        placedCustomIcons.Remove(icon);

        icon.RectTransform.DOScale(Vector3.zero, 0.15f)
            .SetEase(Ease.InBack)
            .OnComplete(() => Destroy(icon.gameObject))
            .SetUpdate(true);

        DeselectIcon();
    }

    public void ClearAllIcons()
    {
        foreach (var icon in placedCustomIcons)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }
        placedCustomIcons.Clear();
        DeselectIcon();
    }

    #endregion Icon Placement

    #region Icon Selection

    private void CheckForNearbyIcons()
    {
        if (selectedIcon != null) return;

        MapIcon nearestIcon = GetNearestIcon(cursorIcon.anchoredPosition, cursorSnapDistance);

        foreach (var icon in placedCustomIcons)
        {
            icon.SetHighlight(icon == nearestIcon);
        }
        foreach (var icon in placedWorldIcons)
        {
            icon.SetHighlight(icon == nearestIcon);
        }
    }

    private MapIcon GetNearestIcon(Vector2 position, float maxDistance)
    {
        MapIcon nearest = null;
        float nearestDistance = maxDistance;

        foreach (var icon in placedCustomIcons)
        {
            if (icon == null) continue;

            float distance = Vector2.Distance(position, icon.RectTransform.anchoredPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = icon;
            }
        }

        foreach (var icon in placedWorldIcons)
        {
            if (icon == null) continue;

            float distance = Vector2.Distance(position, icon.RectTransform.anchoredPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = icon;
            }
        }

        return nearest;
    }

    private void SelectIcon(MapIcon icon)
    {
        if (icon == null) return;

        selectedIcon = icon;
        selectedIcon.SetSelected(true);

        cursorIcon.DOKill();
        targetCursorPosition = icon.RectTransform.anchoredPosition;
        cursorIcon.DOAnchorPos(targetCursorPosition, 0.15f).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    private void DeselectIcon()
    {
        if (selectedIcon != null)
        {
            selectedIcon.SetSelected(false);
            selectedIcon = null;
            isSelectingIcon = false;
        }
    }

    #endregion Icon Selection

    #region Public Getters

    public bool IsMapOpen => isMapOpen;

    public List<MapIcon> GetPlacedIcons() => new List<MapIcon>(placedCustomIcons);

    public Sprite GetIconByType(MapIconType type)
    {
        switch (type)
        {
            case MapIconType.Player: return playerIconSprite;
            case MapIconType.Shop: return shopIconSprite;
            case MapIconType.Quest: return questIconSprite;
            case MapIconType.SavePoint: return savePointIconSprite;
            case MapIconType.Custom1: return customIconSprite1;
            case MapIconType.Custom2: return customIconSprite2;
            case MapIconType.Custom3: return customIconSprite3;
            case MapIconType.Custom4: return customIconSprite4;
            default: return customIconSprite1;
        }
    }

    #endregion Public Getters

    private void OnDrawGizmosSelected()
    {
        float Ax = worldPointBotLeft.x;
        float Ay = worldPointBotLeft.y;
        float Bx = worldPointTopRight.x;
        float By = worldPointTopRight.y;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector2(Ax, Ay), new Vector2(Bx, Ay));
        Gizmos.DrawLine(new Vector2(Ax, Ay), new Vector2(Ax, By));
        Gizmos.DrawLine(new Vector2(Bx, By), new Vector2(Ax, By));
        Gizmos.DrawLine(new Vector2(Bx, By), new Vector2(Bx, Ay));
    }
}

/// <summary>
/// Represents a placeable icon on the map
/// </summary>
public class MapIcon : MonoBehaviour
{
    public RectTransform RectTransform { get; private set; }
    public Vector2 originalPosition;

    [SerializeField] private Image iconImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color selectedColor = Color.cyan;

    private bool isSelected;
    private bool isHighlighted;

    [HideProperty] public MapIconType iconType;
    [HideProperty] public GameObject worldGameobject;

    public void Initialize(RectTransform rectTransform, Vector2 _originalPosition)
    {
        RectTransform = rectTransform;
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
        }
        originalPosition = _originalPosition;
        UpdateVisual();
    }

    public void SetHighlight(bool highlight)
    {
        if (isSelected) return;

        isHighlighted = highlight;
        UpdateVisual();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();

        if (isSelected)
        {
            RectTransform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        else
        {
            RectTransform.DOScale(1f, 0.1f).SetEase(Ease.OutCubic).SetUpdate(true);
        }
    }

    private void UpdateVisual()
    {
        if (iconImage == null) return;

        if (isSelected)
        {
            iconImage.color = selectedColor;
        }
        else if (isHighlighted)
        {
            iconImage.color = highlightColor;
        }
        else
        {
            iconImage.color = normalColor;
        }
    }
}