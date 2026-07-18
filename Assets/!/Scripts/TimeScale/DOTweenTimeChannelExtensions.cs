using DG.Tweening;
using UnityEngine;

public static class DOTweenTimeChannelExtensions
{
    public static Tween SetTimeDt(this Tween tween, MonoBehaviour host, TimeChannel channel)
    {
        if (tween == null || host == null) return tween;

        tween.SetUpdate(true); // unscaled driver
        TimeChannelTweenBinder.Bind(host, tween, channel);
        return tween;
    }
}