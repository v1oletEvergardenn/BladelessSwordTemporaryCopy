using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLight : MonoBehaviour
{
    private void Start()
    {
        VFXManager.instance.globalLight = this.GetComponent<Light2D>();
    }
}