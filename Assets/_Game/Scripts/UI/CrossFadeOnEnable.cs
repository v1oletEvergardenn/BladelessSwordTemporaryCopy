using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CrossFadeOnEnable : MonoBehaviour
{
    public float duration = 0.5f;
    public float alpha = 1f;

    private void OnEnable()
    {
        GetComponent<Image>().DOComplete();
        Color tempColor = GetComponent<Image>().color;
        tempColor.a = 0;
        GetComponent<Image>().color = tempColor;

        GetComponent<Image>().DOFade(alpha, duration).SetTimeDt(this, TimeChannel.UI);
    }
}