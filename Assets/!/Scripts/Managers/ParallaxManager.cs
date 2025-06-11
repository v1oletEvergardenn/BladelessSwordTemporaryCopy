using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParallaxManager : MonoBehaviour
{
    public Camera cam;
    public List<layer> layers = new List<layer>();

    public void Start()
    {
        foreach (layer i in layers)
        {
            i.startPos = i.trans.position.x;
            i.sprite = i.trans.GetComponent<SpriteRenderer>();
            i.spriteLength = i.sprite.bounds.size.x;
            if (i.repeat)
            {
                GameObject prefab = i.trans.gameObject;
                i.leftCopy = Instantiate(prefab, i.trans).transform;
                i.rightCopy = Instantiate(i.leftCopy.gameObject, i.trans).transform;

                i.rightCopy.transform.localPosition = new Vector3(i.spriteLength, 0, 0);
                i.leftCopy.transform.localPosition = new Vector3(-i.spriteLength, 0, 0);
            }
        }
    }

    public void FixedUpdate()
    {
        foreach (layer i in layers)
        {
            float dist = cam.transform.position.x * i.movingSpeed;
            float movement = cam.transform.position.x * (1 - i.movingSpeed);
            i.trans.position = new Vector3(i.startPos + dist, i.trans.position.y, i.trans.position.z);

            if (i.repeat)
            {
                if (movement > i.startPos + i.spriteLength) { i.startPos += i.spriteLength; }
                else if (movement < i.startPos - i.spriteLength) { i.startPos -= i.spriteLength; }
            }
        }
    }
}

[Serializable]
public class layer
{
    public Transform trans;
    [HideInInspector] public Transform leftCopy;
    [HideInInspector] public Transform rightCopy;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public float spriteLength;
    public float startPos;
    [Range(0, 1)] public float movingSpeed;
    public bool repeat;
}