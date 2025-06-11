using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDisactive : MonoBehaviour
{
    public float disactiveTime = 10f;

    private void Start()
    {
        Invoke("DisActive", disactiveTime);
    }

    public void DisActive()
    {
        gameObject.SetActive(false);
    }
}