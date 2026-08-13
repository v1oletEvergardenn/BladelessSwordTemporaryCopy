using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishCameraFollowCondition : CameraFollowCondition
{
    public Transform waterLevel;

    public override bool CheckCameraFollowCondition()
    {
        if (waterLevel != null)
        {
            if (transform.position.y < waterLevel.position.y)
            {
                return false;
            }
        }
        return true;
    }
}