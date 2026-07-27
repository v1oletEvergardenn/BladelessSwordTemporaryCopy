using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QinAnimatorEvent : MonoBehaviour
{
    public void SyncRunningAnim()
    {
        CharacterController2D.instance.SyncRunningAnim();
    }
}