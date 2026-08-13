using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSword : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((PlayerControl.instance.teleportCheckLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            PlayerControl.instance.TeleportToSword();
            StopCoroutine(PlayerControl.instance.co_teleport);
        }
    }
}