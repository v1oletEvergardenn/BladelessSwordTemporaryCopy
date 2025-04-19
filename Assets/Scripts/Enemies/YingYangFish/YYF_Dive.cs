using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

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
        //dive
        bool next1 = false;
        bool ToLeft = true;

        swimEffect.transform.localScale = Vector3.one;

        bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(true, false));
        bossAI.StopRotate();
        bossAI.black_rotateSpeed = 0;
        bossAI.white_rotateSpeed = 0;

        bossAI.whiteOrigin.DORotate(new Vector3(0, 0, -90), 1.05f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        bossAI.blackOrigin.DORotate(new Vector3(0, 0, -90), 1f, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        bossAI.whiteOrigin.DOMoveY(bossAI.waterLevel.position.y - 5, 1.05f).SetDelay(0.9f).SetEase(Ease.Linear).OnStart(() => { bossAI.whiteAnim.Play("white_dive_2"); });
        bossAI.blackOrigin.DOMoveY(bossAI.waterLevel.position.y - 5, 1f).SetDelay(0.8f).SetEase(Ease.Linear).OnStart(() => { bossAI.blackAnim.Play("black_dive_2"); });

        bossAI.center.DOMoveY(bossAI.waterLevel.position.y - 5, 2f).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            if (bossAI.movingTarget.position.x > transform.position.x) { ToLeft = false; swimEffect.transform.localScale = new Vector3(-1, 1, 1); }
            if (ToLeft) { swimEffect.transform.position = new Vector3(bossAI.movingTarget.position.x + 15, bossAI.waterLevel.position.y, 0); }
            else { swimEffect.transform.position = new Vector3(bossAI.movingTarget.position.x - 15, bossAI.waterLevel.position.y, 0); }
            transform.DOMove(new Vector3(swimEffect.transform.position.x, bossAI.waterLevel.position.y - 6, 0), 1f).OnComplete(() =>
            {
                bossAI.whiteOrigin.localPosition = Vector3.zero;
                bossAI.blackOrigin.localPosition = Vector3.zero;
                bossAI.center.localPosition = Vector3.zero; next1 = true;
            });
        });

        while (!next1) { yield return null; }

        swimEffect.SetActive(true);

        if (ToLeft) { transform.DOMove(new Vector3(bossAI.movingTarget.position.x + 5, bossAI.waterLevel.position.y - 7, 0), 1f); }
        else { transform.DOMove(new Vector3(bossAI.movingTarget.position.x - 5, bossAI.waterLevel.position.y - 7, 0), 1f); }

        //play under water animation while moving
        while (Mathf.Abs(swimEffect.transform.position.x - bossAI.movingTarget.position.x) > 4)
        {
            if (ToLeft) { swimEffect.transform.position -= new Vector3(10, 0, 0) * Time.deltaTime * 2; }
            else { swimEffect.transform.position += new Vector3(10, 0, 0) * Time.deltaTime * 2; }
            yield return null;
        }
        //jump out

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

        bool isSwimming = false;

        if (ToLeft)
        {
            transform.DOMove(new Vector3(bossAI.movingTarget.position.x - 1, bossAI.waterLevel.position.y + 6.5f, 0), 1f).SetEase(Ease.OutCubic);
        }
        else
        {
            transform.DOMove(new Vector3(bossAI.movingTarget.position.x + 1, bossAI.waterLevel.position.y + 6.5f, 0), 1f).SetEase(Ease.OutCubic);
        }

        while (transform.position.y < bossAI.waterLevel.position.y + 6.5)
        {
            if (transform.position.y > bossAI.waterLevel.position.y - 1)
            {
                swimEffect.GetComponent<Animator>().Play("end");
                if (!isSwimming && (bossAI.actionList.Count <= 1 || bossAI.actionList[1] != bossAI.swing))
                {
                    isSwimming = true;
                    //bossAI.co_IEcloseSwim = StartCoroutine(bossAI.IECloseSwim(false));
                }
            }
            if (transform.position.y > bossAI.waterLevel.position.y + 4)
            {
                bossAI.blackAnim.SetBool("dive_end", true);
                bossAI.whiteAnim.SetBool("dive_end", true);
            }

            yield return null;
        }

        Vector3 dest = transform.position - new Vector3(0, 2, 0);
        Vector3 origin = transform.position;
        float elpasedTime = 0f;

        while (transform.position.y > bossAI.waterLevel.position.y + 4.5)
        {
            transform.position = Vector3.Lerp(origin, dest, elpasedTime / 0.5f);
            elpasedTime += Time.deltaTime;

            yield return null;
        }

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
            bossAI.black_targetRotateSpeed = bossAI.idleRotateSpeed;
        }
        else
        {
            bossAI.white_targetRotateSpeed = bossAI.idleRotateSpeed;
        }
    }

    public void Sprint()
    {
        bossAI.co_sprintBackEqual = StartCoroutine(bossAI.SprintBackEqual());
    }
}