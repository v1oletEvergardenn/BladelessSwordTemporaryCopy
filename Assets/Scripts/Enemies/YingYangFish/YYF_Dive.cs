using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class YYF_Dive : IEnemyAction
{
    public YingYangFish_AI bossAI;
    public GameObject swimEffect;
    public Transform dive_end_pos;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override IEnumerator Act_coroutine()
    {
        print(1);
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

        TryStopCoroutine(bossAI.co_sprintStartPoint);
        TryStopCoroutine(bossAI.co_IEcloseSwim);
        TryStopCoroutine(bossAI.co_sprintBackEqual);

        bossAI.black_idling = false;
        bossAI.white_idling = false;
        bossAI.whiteAnim.Play("white_dive_1");
        bossAI.blackAnim.Play("black_dive_1");

        bossAI.blackFish.eulerAngles = Vector3.zero;
        bossAI.blackFish.localPosition = new Vector3(0, 1, 0);
        bossAI.whiteFish.eulerAngles = Vector3.zero;
        bossAI.whiteFish.localPosition = new Vector3(0, 1, 0);
        bossAI.blackFish.RotateAround(transform.position, bossAI.Dir, 210);
        bossAI.whiteFish.RotateAround(transform.position, bossAI.Dir, 210);

        bool isSwimming = false;

        yield return new WaitForSeconds(0.5f);

        while (transform.position.y < bossAI.waterLevel.position.y + 4.5)
        {
            if (transform.position.y > bossAI.waterLevel.position.y - 1)
            {
                //swimEffect.GetComponent<Animator>().Play("end");
                if (!isSwimming && (bossAI.actionList.Count <= 1 || bossAI.actionList[1] != bossAI.swing))
                {
                    isSwimming = true;
                    //bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
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
        bossAI.blackAnim.SetBool("dive_end", true);
        bossAI.whiteAnim.SetBool("dive_end", true);
        yield return new WaitForSeconds(0.5f);
        //bossAI.co_sprintBackEqual = StartCoroutine(bossAI.SprintBackEqual());
        bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(true));

        if (!bossAI.black_idling || !bossAI.white_idling) { yield return null; }
        yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.SprintBackEqual());

        swimEffect.SetActive(false);
        bossAI.movingTarget = bossAI.player;
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();
        bossAI.blackAnim.SetBool("dive_end", false);
        bossAI.whiteAnim.SetBool("dive_end", false);
        yield return null;
    }

    public void ReturnToNormal(bool isBlack)
    {
        if (isBlack)
        {
            bossAI.black_idling = true;
        }
        else
        {
            bossAI.white_idling = true;
        }
    }
}