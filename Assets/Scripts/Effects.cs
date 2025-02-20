using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effects : MonoBehaviour
{
    private void OnEnable()
    {
        randomRot();
    }

    private void randomRot()
    {
        float i = Random.Range(0f, 360f);
        transform.eulerAngles = new Vector3(0, 0, i);
    }

    public void End()
    {
        this.gameObject.SetActive(false);
    }
}