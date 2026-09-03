using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TJJBaseAction : IEnemyAction
{
    [HideInInspector] public TJJBaseController bossAi;

    public override void Start()
    {
        base.Start();
        bossAi = GetComponent<TJJBaseController>();
    }

    public override void OnActionEnd()
    {
        base.OnActionEnd();
        bossAi.lastAction = this;
    }
}