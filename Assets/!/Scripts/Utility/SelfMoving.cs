using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfMoving : MonoBehaviour
{
    public float Speed = 10f;
    public Vector3 direction = Vector3.zero;

    // Update is called once per frame
    private void Update()
    {
        transform.position += direction.normalized * Speed * Time.deltaTime;
    }
}