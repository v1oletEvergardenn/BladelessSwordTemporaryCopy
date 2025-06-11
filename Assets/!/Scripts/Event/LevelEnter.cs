using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelEnter : MonoBehaviour
{
    public bool oneShot = false;
    private bool alreadyEntered = false;
    private bool alreadyExited = false;
    public LayerMask target;
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (alreadyEntered)
            return;
        if ((target.value & (1 << collision.gameObject.layer)) > 0)
            onTriggerEnter?.Invoke();
        if (oneShot)
            alreadyEntered = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (alreadyExited)
            return;
        if ((target.value & (1 << collision.gameObject.layer)) > 0)
            onTriggerExit?.Invoke();
        if (oneShot)
            alreadyExited = true;
    }

    public void ChangeMapCameraPos()
    {
        InputPlayer.instance.MapCamera.position = this.transform.position;
    }
}