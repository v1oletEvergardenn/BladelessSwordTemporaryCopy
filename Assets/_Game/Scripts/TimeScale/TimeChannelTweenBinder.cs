using System.Collections;
using DG.Tweening;
using UnityEngine;

public static class TimeChannelTweenBinder
{
    public static Coroutine Bind(MonoBehaviour host, Tween tween, TimeChannel channel)
    {
        return host.StartCoroutine(BindRoutine(tween, channel));
    }

    private static IEnumerator BindRoutine(Tween tween, TimeChannel channel)
    {
        while (tween != null && tween.active)
        {
            float unscaled = Time.unscaledDeltaTime;
            float scale = unscaled > 0f
                ? TimeScaleManager.instance.GetDeltaTime(channel) / unscaled
                : 1f;

            tween.timeScale = scale;
            yield return null;
        }
    }
}