using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class tut_sword_storm : MonoBehaviour
{
    public int goal_hit_numbers = 4;
    public Barrier barrier;
    public UnityEvent _event;

    public void Update()
    {
        if (barrier.lastHitNumbers >= goal_hit_numbers)
        {
            _event.Invoke();
        }
    }
}