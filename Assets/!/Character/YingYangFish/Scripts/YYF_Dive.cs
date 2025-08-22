using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class YYF_Dive : IEnemyAction
{
    public YingYangFish_AI bossAI;
    public GameObject swimEffect;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<YingYangFish_AI>();
    }

    public override void CancelAct()
    {
        if (act_routine != null)
        {
            StopCoroutine(act_routine);
        }
        bossAI.blackAnim.SetBool("dive_end", false);
        bossAI.whiteAnim.SetBool("dive_end", false);
    }

    public override bool CanAct()
    {
        return (!bossAI.isWhiteBusy && !bossAI.isBlackBusy);
    }

    public override IEnumerator Act_coroutine(float factor = 0)
    {
        //dive
        bool next1 = false;
        bool ToLeft = true;

        Vector3 pos = bossAI.movingTarget.transform.position;
        bossAI.SetBlackBusy(); bossAI.SetWhiteBusy();

        if (factor == 0 && (Mathf.Abs(pos.x - transform.position.x) <= 2))
        {
            pos = bossAI.movingTarget.transform.position;
            Debug.Log("too close, cancel action");
        }
        else
        {
            yield return bossAI.co_sprintBackEqual = StartCoroutine(bossAI.IESprintBackEqual());
            bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(true));
            yield return new WaitForSeconds(0.2f);
            bossAI.SetBlackTargetRotateSpeed(bossAI.sprintRotateSpeed);
            bossAI.SetWhiteTargetRotateSpeed(bossAI.sprintRotateSpeed);

            //dive
            transform.DOMoveY(pos.y - 5, 2f).SetEase(Ease.InOutBack).OnComplete(() =>
            {
                if (pos.x > transform.position.x) { ToLeft = false; swimEffect.transform.localScale = new Vector3(-1, 1, 1); }
                else { swimEffect.transform.localScale = new Vector3(1, 1, 1); }
                pos = bossAI.movingTarget.transform.position;
                if (ToLeft) { swimEffect.transform.position = new Vector3(pos.x + 15, bossAI.waterLevel.position.y, 0); }
                else { swimEffect.transform.position = new Vector3(pos.x - 15, bossAI.waterLevel.position.y, 0); }
                transform.DOMove(new Vector3(swimEffect.transform.position.x, bossAI.waterLevel.position.y - 6, 0), 1f).OnComplete(() =>
                {
                    bossAI.whiteOrigin.localPosition = Vector3.zero;
                    bossAI.blackOrigin.localPosition = Vector3.zero;
                    next1 = true;
                });
            });

            while (!next1) { yield return null; }

            swimEffect.SetActive(true);

            pos = bossAI.movingTarget.transform.position;

            if (ToLeft) { transform.DOMove(new Vector3(pos.x + 5, bossAI.waterLevel.position.y - 7, 0), 1f); }
            else { transform.DOMove(new Vector3(pos.x - 5, bossAI.waterLevel.position.y - 7, 0), 1f); }

            //swim effect moving
            while (Mathf.Abs(swimEffect.transform.position.x - pos.x) > 4)
            {
                if (ToLeft) { swimEffect.transform.position -= new Vector3(10, 0, 0) * Time.deltaTime * 2; }
                else { swimEffect.transform.position += new Vector3(10, 0, 0) * Time.deltaTime * 2; }
                yield return null;
            }

            // prepare to jump out
            swimEffect.GetComponent<Animator>().Play("inverse");
            TryStopCoroutine(bossAI.co_sprintStartPoint);
            TryStopCoroutine(bossAI.co_IEcloseSwim);
            TryStopCoroutine(bossAI.co_sprintBackEqual);
            bossAI.whiteOrigin.eulerAngles = Vector3.zero;
            bossAI.blackOrigin.eulerAngles = Vector3.zero;

            bossAI.StopRotate();

            bossAI.whiteAnim.Play("white_dive_1");
            bossAI.blackAnim.Play("black_dive_1");
            bossAI.blackFish.localPosition = new Vector3(0, 1, 0);
            bossAI.whiteFish.localPosition = new Vector3(0, 1, 0);

            bossAI.whiteFishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
            bossAI.whiteFishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);
            bossAI.blackFishGFX.DOLocalRotate(new Vector3(0, 0, 0), 0.1f);
            bossAI.blackFishGFX.DOLocalMove(new Vector3(0, 0, 0), 0.1f);

            float angle = 210;
            if (!ToLeft) { angle += 90; }

            bossAI.blackOrigin.Rotate(bossAI.Dir, angle);
            bossAI.whiteOrigin.Rotate(bossAI.Dir, angle);

            pos = bossAI.movingTarget.transform.position;

            //jump out
            float x = ToLeft ? pos.x - 1 : pos.x + 1;
            transform.DOMove(new Vector3(x, bossAI.waterLevel.position.y + 4.5f, 0), 1f).SetEase(Ease.OutCubic);

            // during jumping out
            while (transform.position.y < bossAI.waterLevel.position.y + 4.5f)
            {
                if (transform.position.y > bossAI.waterLevel.position.y - 1)
                {
                    swimEffect.GetComponent<Animator>().Play("end");
                }
                if (transform.position.y > bossAI.waterLevel.position.y + 2)
                {
                    bossAI.blackAnim.SetBool("dive_end", true);
                    bossAI.whiteAnim.SetBool("dive_end", true);
                }

                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }

        swimEffect.SetActive(false);
        bossAI.movingTarget = bossAI.player;
        bossAI.AddActionBreak(actionBreakAmount);
        bossAI.EndAction();
        bossAI.blackAnim.SetBool("dive_end", false);
        bossAI.whiteAnim.SetBool("dive_end", false);
        bossAI.SetBlackNotBusy();
        bossAI.SetWhiteNotBusy();
        yield return null;
    }

    public void ReturnToNormal(bool isBlack)
    {
        if (isBlack)
        {
            bossAI.SetBlackTargetRotateSpeed(bossAI.idleRotateSpeed);
        }
        else
        {
            bossAI.SetWhiteTargetRotateSpeed(bossAI.idleRotateSpeed);
        }
    }

    public IEnumerator CloseSwim()
    {
        while (bossAI.white_distanceToCenter > bossAI.minMaxDistanceTocenter.x)
        {
            bossAI.whiteFish.position -= bossAI.whiteFish.up * bossAI.swimToCenterSpeed * Time.deltaTime;
            bossAI.blackFish.position -= bossAI.blackFish.up * bossAI.swimToCenterSpeed * Time.deltaTime;
            yield return null;
        }
    }

    public void Sprint()
    {
        return;
        bossAI.co_sprintBackEqual = StartCoroutine(bossAI.IESprintBackEqual());
    }
}