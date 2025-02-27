using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewWaterBossController : MonoBehaviour
{
    public Transform center;
    public Transform blackFish;
    public Transform whiteFish;

    public float idleSpeed;
    public float idleRotateSpeed;
    public bool idle_black = true;
    private Rigidbody2D rb_black;

    public Vector3 Dir;

    // Start is called before the first frame update
    private void Start()
    {
        rb_black = blackFish.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (idle_black)
        {
            blackFish.RotateAround(transform.position, Dir, idleRotateSpeed * Time.deltaTime);
            whiteFish.RotateAround(transform.position, Dir, idleRotateSpeed * Time.deltaTime);
        }
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }
}