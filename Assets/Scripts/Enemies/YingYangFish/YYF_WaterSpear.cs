using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_WaterSpear : IEnemyAction
{
    private YingYangFish_AI bossAI;

    public override void Start()
    {
        base.Start();
        bossAI.GetComponent<YingYangFish_AI>();
    }
}