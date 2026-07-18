using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDisactive : MonoBehaviour
{
    public float disactiveTime = 10f;

    public bool useAnim = false;
    [ShowIf(nameof(useAnim))] public string endAnimName = "end";
    [ShowIf(nameof(useAnim))] public float animTime;

    private Coroutine co_disactive;
    private Coroutine co_animDisactive;

    private void OnEnable()
    {
        if (!useAnim) co_disactive = StartCoroutine(DisActive(disactiveTime));
        else co_animDisactive = StartCoroutine(AnimBeforeDisActive(disactiveTime));
    }

    public void DisActive()
    {
        gameObject.SetActive(false);
    }

    public void AnimbeforeDisActive()
    {
        GetComponent<Animator>().Play(endAnimName);
        Invoke("DisActive", animTime);
    }

    public IEnumerator DisActive(float time)
    {
        yield return TimeScaleManager.WaitForChannelSeconds(time, TimeChannel.Gameplay);
        gameObject.SetActive(false);
    }

    public IEnumerator AnimBeforeDisActive(float time)
    {
        yield return TimeScaleManager.WaitForChannelSeconds(time, TimeChannel.Gameplay);
        GetComponent<Animator>().Play(endAnimName);
        co_disactive = StartCoroutine(DisActive(animTime));
    }

    public void SetNewDisActiveTime(float _disactiveTime)
    {
        disactiveTime = _disactiveTime;
        TryStopCoroutine(co_disactive);
        if (useAnim) TryStopCoroutine(co_animDisactive);
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void TryStopCoroutine(Coroutine co)
    {
        if (co != null) StopCoroutine(co);
    }
}