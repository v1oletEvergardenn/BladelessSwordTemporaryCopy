using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : EventObject
{
    public override void InteractEvent()
    {
        _Event?.Invoke();
        //save game
        SaveSystem.Save();
        //show saved UI
        //rumble
        VFXManager.instance.RumblePulse(0.3f, 0.5f, 0.2f);
    }
}