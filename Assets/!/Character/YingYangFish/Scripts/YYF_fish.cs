using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YYF_fish : MonoBehaviour
{
    public YingYangFish_AI bossAI;
    public bool isBlack;

    public Transform fish;
    public Transform fishGFX;
    public Animator anim;
    public SubDamageable subDamageable;

    // Start is called before the first frame update
    private void Start()
    {
        bossAI = YingYangFish_AI.instance;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, -1), isBlack ? bossAI.black_rotateSpeed : bossAI.white_rotateSpeed * Time.deltaTime);
    }

    public void SetDamageableParent(IDamagable parent)
    {
        subDamageable.SetParent(parent);
    }
}