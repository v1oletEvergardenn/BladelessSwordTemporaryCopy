using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IEnemyActionIdle : IEnemyAction
{
    public float idleTime = 2f;

    public override IEnumerator Act_coroutine()
    {
        yield return new WaitForSeconds(idleTime);
        controller.NextAction();
        yield return null;
    }
}