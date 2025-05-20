using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAtSpeed : MonoBehaviour
{
    public float speed = 100f;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }
}