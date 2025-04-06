using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_Dive : IEnemyAction
{
    public YingYangFish_AI bossAI;
    public GameObject swimEffect;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override IEnumerator Act_coroutine()
    {
        //dive
        while (transform.position.y > bossAI.waterLevel.position.y - 9) { transform.position -= new Vector3(0, 1, 0) * Time.deltaTime * 7; yield return null; }

        bool ToLeft = true;
        swimEffect.transform.localScale = Vector3.one;
        if (bossAI.movingTarget.position.x > transform.position.x) { ToLeft = false; swimEffect.transform.localScale = new Vector3(-1, 1, 1); }

        if (ToLeft)
        {
            swimEffect.transform.position = new Vector3(bossAI.movingTarget.position.x + 15, bossAI.waterLevel.position.y, 0);
        }
        else
        {
            swimEffect.transform.position = new Vector3(bossAI.movingTarget.position.x - 15, bossAI.waterLevel.position.y, 0);
        }
        swimEffect.SetActive(true);

        bossAI.whiteAnim.Play("close_swim");
        bossAI.blackAnim.Play("close_swim");
        bossAI.whiteFish.eulerAngles = Vector3.zero;
        bossAI.blackFish.eulerAngles = new Vector3(0, 0, 180);
        bossAI.whiteFish.localPosition = new Vector3(0, 1, 0);
        bossAI.blackFish.localPosition = new Vector3(0, -1, 0);

        //StartCoroutine(bossAI.SprintBackEqual());
        //bossAI.whiteAnim.Play("close_swim");
        //bossAI.blackAnim.Play("close_swim");
        //bossAI.whiteFish.localPosition -= bossAI.whiteFish.up * 2;
        //bossAI.blackFish.localPosition -= bossAI.blackFish.up * 2;
        bool isSwimming = false;

        //play under water animation while moving
        while (Mathf.Abs(swimEffect.transform.position.x - bossAI.movingTarget.position.x) > 4)
        {
            transform.position = new Vector3(swimEffect.transform.position.x, transform.position.y, 0);
            if (ToLeft) { swimEffect.transform.position -= new Vector3(10, 0, 0) * Time.deltaTime * 2; }
            else { swimEffect.transform.position += new Vector3(10, 0, 0) * Time.deltaTime * 2; }
            yield return null;
        }
        //jump out
        if (ToLeft) { transform.position = swimEffect.transform.position + new Vector3(3, -7, 0); }
        else { transform.position = swimEffect.transform.position + new Vector3(-3, -7, 0); }
        swimEffect.GetComponent<Animator>().Play("inverse");
        yield return new WaitForSeconds(0.5f);

        while (transform.position.y < bossAI.waterLevel.position.y + 4.5)
        {
            if (transform.position.y > bossAI.waterLevel.position.y - 1)
            {
                swimEffect.GetComponent<Animator>().Play("end");
                if (!isSwimming && (bossAI.actionList.Count <= 1 || bossAI.actionList[1] != bossAI.swing))
                {
                    isSwimming = true;
                    bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
                }
            }
            if (ToLeft)
            {
                transform.position += new Vector3(-3, 7).normalized * Time.deltaTime * 20;
            }
            else
            {
                transform.position += new Vector3(3, 7).normalized * Time.deltaTime * 20;
            }
            yield return null;
        }
        swimEffect.SetActive(false);
        bossAI.movingTarget = bossAI.player;
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();
        yield return null;
    }
}