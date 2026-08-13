using EditorAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public abstract class EventObject : MonoBehaviour
{
    public UnityEvent _Event;
    public GameObject interactSprite;
    public Image normalKey;
    public Image InteractedRing;
    public bool HoldToInteract;
    [ShowField(nameof(HoldToInteract))] public float holdingTime;
    [HideInInspector] public float holdingTimer;
    [HideInInspector] public bool isHolding = false;

    public bool HoldRumble;
    [ShowField(nameof(HoldRumble)), MinMaxSlider(0, 2f)] public Vector2 rumbleFrequency;

    // Start is called before the first frame update

    private void Start()
    {
        interactSprite.SetActive(false);
    }

    public virtual void InteractEvent()
    {
        _Event?.Invoke();
    }

    public virtual void ShowInteractSign(bool b)
    {
        EndInteraction();
        interactSprite.gameObject.SetActive(b);
    }

    public virtual void Update()
    {
        if (isHolding)
        {
            holdingTimer += TimeScaleManager.UIDt;
            InteractedRing.fillAmount = holdingTimer / holdingTime;
        }
    }

    public virtual void Interact(bool interact)
    {
        if (interact)
        {
            if (!HoldToInteract)
            {
                InteractedRing.fillAmount = 1;
                InteractEvent();
                EndInteraction();
            }
            else
            {
                isHolding = true;
                if (HoldRumble) { VFXManager.instance.Rumble(rumbleFrequency.x, rumbleFrequency.y); }
            }
        }
        else
        {
            if (HoldToInteract)
            {
                if (holdingTimer >= holdingTime)
                {
                    InteractEvent();
                }
            }

            EndInteraction();
        }
    }

    public virtual void EndInteraction()
    {
        isHolding = false;
        InteractedRing.fillAmount = 0;
        holdingTimer = 0f;
        if (HoldToInteract) VFXManager.instance.StopRumble();
    }
}