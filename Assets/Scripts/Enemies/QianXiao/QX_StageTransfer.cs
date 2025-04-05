using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class QX_StageTransfer : IEnemyAction
{
    private QianXiaoAI bossAI;
    public Transform teleportPos;
    public GameObject wings;
    public float preparingTime = 10f;
    public GameObject stageTransferCol;
    public List<Transform> BubblesPos;
    private List<EnergyBubble> bubbles = new List<EnergyBubble>();
    public GameObject beam;

    public override void Start()
    {
        base.Start();
        bossAI = GetComponent<QianXiaoAI>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    //public override IEnumerator Act_coroutine()
    //{
    //    isThisActing = true;
    //    anim.Play("S1_teleport");
    //    bossAI.isStage2 = true;
    //    bossAI.Stage2HealthChange();
    //    bossAI.currentStun = 0;
    //    bossAI.Stun(0);
    //    yield return new WaitForSeconds(0.2f);
    //    rb.gravityScale = 0f;
    //    bossAI.Teleport(teleportPos.position);
    //    yield return new WaitForSeconds(2f);
    //    wings.SetActive(false);
    //    bossAI.OutLine_Activate(1);
    //    stageTransferCol.SetActive(true);
    //    anim.Play("StageTransferPreparing");

    //    //generate energies
    //    GenerateEnergyBubbles();
    //    yield return new WaitForSeconds(preparingTime);
    //    stageTransferCol.SetActive(false);
    //    anim.Play("S1_to_S2");
    //    bossAI.SetFlyEngine(true);
    //    bossAI.OutLine_Activate(0);
    //    foreach (EnergyBubble b in bubbles)
    //    {
    //        b.End();
    //    }
    //    yield return new WaitForSeconds(2f);
    //    beam.SetActive(true);
    //    yield return new WaitForSeconds(5f);
    //    beam.SetActive(false);
    //    yield return new WaitForSeconds(action_time);
    //    isThisActing = false;
    //    bossAI.CancelAllActions();
    //    bossAI.add_land_attack.Act();
    //    bossAI.add_land_attack.GetComponent<QX_add_land_attack>().actionSender = this;
    //    yield return null;
    //}

    public void GenerateEnergyBubbles()
    {
        bubbles.Clear();
        foreach (Transform t in BubblesPos)
        {
            EnergyBubble bubble = pooler.SpawnFromPool("energy_bubble", t.position).GetComponent<EnergyBubble>();
            bubble.SetUp(new Vector3(0, 0, 90), bossAI.gameObject, _damage: 1, _speed: 30);
            bubbles.Add(bubble);
        }
    }

    public IEnumerator IE_Interrupt()
    {
        StopCoroutine(Act_coroutine());
        foreach (EnergyBubble b in bubbles)
        {
            b.End();
        }
        stageTransferCol.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        anim.Play("S1_to_S2");
        bossAI.SetFlyEngine(true);
        bossAI.OutLine_Activate(0);
        bossAI.CancelAllActions();
        bossAI.add_land_attack.Act();
        bossAI.add_land_attack.GetComponent<QX_add_land_attack>().actionSender = this;
    }

    public void Interrupt()
    {
        StartCoroutine(IE_Interrupt());
    }
}