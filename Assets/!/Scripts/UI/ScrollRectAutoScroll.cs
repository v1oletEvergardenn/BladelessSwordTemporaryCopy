using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectAutoScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scrollSpeed = 10f;
    private bool mouseOver = false;

    public List<Selectable> m_Selectables = new List<Selectable>();
    public ScrollRect m_ScrollRect;

    public Vector2 m_NextScrollPosition = Vector2.up;

    private void OnEnable()
    {
        if (m_ScrollRect)
        {
            m_ScrollRect.content.GetComponentsInChildren(m_Selectables);
        }
    }

    private void Awake()
    {
        m_ScrollRect = GetComponent<ScrollRect>();
    }

    private void Start()
    {
        if (m_ScrollRect)
        {
            m_ScrollRect.content.GetComponentsInChildren(m_Selectables);
        }
        ScrollToSelected(false);
    }

    private void Update()
    {
        // If we are on mobile and we do not have a gamepad connected, do not do anything.
        if (SystemInfo.deviceType == DeviceType.Handheld && Gamepad.all.Count <= 1)
        {
            return;
        }

        // Scroll via input.
        InputScroll();
        if (!mouseOver)
        {
            // Lerp scrolling code.
            m_ScrollRect.normalizedPosition = Vector2.Lerp(m_ScrollRect.normalizedPosition, m_NextScrollPosition, scrollSpeed * Time.unscaledDeltaTime);
        }
        else
        {
            m_NextScrollPosition = m_ScrollRect.normalizedPosition;
        }
    }

#nullable enable

    private void InputScroll()
    {
        if (m_Selectables.Count > 0)
        {
            Keyboard? currentKeyboard = Keyboard.current;
            Gamepad? currentGamepad = Gamepad.current;

            if (currentKeyboard != null)
            {
                if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                {
                    ScrollToSelected(false);
                }
            }

            if (currentGamepad != null)
            {
                // D-Pad or Left Stick vertical movement
                bool dpadPressed = Gamepad.current.dpad.up.isPressed || Gamepad.current.dpad.down.isPressed;
                bool leftStickMoved = Mathf.Abs(Gamepad.current.leftStick.ReadValue().y) > 0.5f;

                if (dpadPressed || leftStickMoved)
                {
                    ScrollToSelected(false);
                }
            }
        }
    }

#nullable disable

    public void ScrollToSelected(bool quickScroll)
    {
        int selectedIndex = -1;
        Selectable selectedElement = EventSystem.current.currentSelectedGameObject ? EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() : null;

        if (selectedElement)
        {
            selectedIndex = m_Selectables.IndexOf(selectedElement);
        }

        if (selectedIndex > -1)
        {
            if (quickScroll)
            {
                m_ScrollRect.normalizedPosition = new Vector2(0, 1 - (selectedIndex / ((float)m_Selectables.Count - 1)));
                m_NextScrollPosition = m_ScrollRect.normalizedPosition;
            }
            else
            {
                m_NextScrollPosition = new Vector2(0, 1 - (selectedIndex / ((float)m_Selectables.Count - 1)));
            }
        }
        if (selectedElement == null)
        {
            m_ScrollRect.verticalNormalizedPosition = 1f;
            m_NextScrollPosition = m_ScrollRect.normalizedPosition;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        ScrollToSelected(false);
    }
}