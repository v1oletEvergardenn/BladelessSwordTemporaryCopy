using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_Swing : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public GameObject swingEffect;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override IEnumerator Act_coroutine()
    {
        bossAI.whiteAnim.Play("swing");
        bossAI.blackAnim.Play("swing");
        bossAI.black_idling = false;
        bossAI.white_idling = false;

        yield return new WaitForSeconds(1f);

        swingEffect.transform.eulerAngles = bossAI.whiteFish.eulerAngles;
        swingEffect.SetActive(true);

        yield return new WaitForSeconds(.7f);

        swingEffect.SetActive(false);
        bossAI.black_idling = true;
        bossAI.white_idling = true;
        bossAI.nextAction = null;
        bossAI.FarSwim();
    }
}