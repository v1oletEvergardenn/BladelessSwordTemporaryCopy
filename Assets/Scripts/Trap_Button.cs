using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class Trap_Button : MonoBehaviour
{
    public bool canRecover = false;
    private bool alreadyEntered = false;
    private bool alreadyExited = false;
    public LayerMask target;
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    public Sprite normalSprite;
    public Sprite pressedSprite;
    private SpriteRenderer sprite;

    public AudioClip click;
    public AudioClip unClick;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyEntered)
            return;
        if ((target.value & (1 << collision.gameObject.layer)) > 0)
        {
            onTriggerEnter?.Invoke();
            sprite.sprite = pressedSprite;
            SoundManager.PlaySound(click);
            if (!canRecover) { alreadyEntered = true; }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (alreadyExited)
            return;
        if ((target.value & (1 << collision.gameObject.layer)) > 0 && canRecover)
        {
            onTriggerExit?.Invoke();
            if (canRecover) { sprite.sprite = normalSprite; SoundManager.PlaySound(unClick); }
        }
    }
}