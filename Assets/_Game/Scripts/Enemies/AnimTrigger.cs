using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimTrigger : MonoBehaviour
{
    public UnityEvent act1;
    public UnityEvent act2;
    public UnityEvent act3;
    public UnityEvent act4;
    public UnityEvent act5;
    public UnityEvent act6;

    // Start is called before the first frame update
    public void Act1()
    {
        act1.Invoke();
    }

    public void Act2()
    {
        act2.Invoke();
    }

    public void Act3()
    {
        act3.Invoke();
    }

    public void Act4()
    {
        act4.Invoke();
    }

    public void Act5()
    {
        act5.Invoke();
    }

    public void Act6()
    {
        act6.Invoke();
    }
}