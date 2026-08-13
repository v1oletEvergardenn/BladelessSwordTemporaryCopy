using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraFollowCondition : MonoBehaviour
{
    public virtual bool CheckCameraFollowCondition()
    {
        return true;
    }
}