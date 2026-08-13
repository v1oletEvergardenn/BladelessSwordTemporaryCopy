using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAssignCanvasCamera : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }

    private void Update()
    {
        canvas.worldCamera = Camera.main;
    }
}