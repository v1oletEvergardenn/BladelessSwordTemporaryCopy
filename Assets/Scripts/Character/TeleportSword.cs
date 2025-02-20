using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSword : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((CharacterController2D.instance.teleportCheckLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            CharacterController2D.instance.TeleportToSword();
        }
    }
}