using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterFall : MonoBehaviour
{
    private InternalObjectPooler pooler;

    [MinMaxSlider(0, 5)] public Vector2 MinMaxGap = new Vector2(5, 15);

    private float gapTimer = 0f;
    private float gapTime = 0f;
    private float length;

    [MinMaxSlider(0, 30)] public Vector2 MinMax_movingSpeed = new Vector2(10, 20);
    public float spawnHeight = 20;

    // Start is called before the first frame update
    private void Start()
    {
        pooler = GetComponent<InternalObjectPooler>();
        gapTime = Random.Range(MinMaxGap.x, MinMaxGap.y);
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    private void Update()
    {
        gapTimer += TimeScaleManager.GameplayDt;
        if (gapTimer >= gapTime)
        {
            gapTimer = 0;
            gapTime = Random.Range(MinMaxGap.x, MinMaxGap.y);

            float x = Random.Range(-length / 2 + 8, length / 2 - 8);
            GameObject wave = pooler.SpawnFromPool("wave", new Vector3(0, 0, 0));
            wave.transform.localPosition = new Vector3(x, spawnHeight);
            wave.GetComponent<SelfMoving>().Speed = Random.Range(MinMax_movingSpeed.x, MinMax_movingSpeed.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(transform.position + new Vector3(-length / 2 + 8, spawnHeight, 0), transform.position + new Vector3(length / 2 - 8, spawnHeight, 0));
    }
}