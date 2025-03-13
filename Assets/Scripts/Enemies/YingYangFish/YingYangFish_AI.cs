using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;

public class YingYangFish_AI : IEnemyController
{
    [FoldoutGroup("Attributes", nameof(center), nameof(blackFish), nameof(whiteFish),
        nameof(idleRotateSpeed), nameof(sprintRotateSpeed), nameof(Dir))]
    public Void void2;

    [SerializeField, HideInInspector] public Transform center;
    [SerializeField, HideInInspector] public Transform blackFish;
    [SerializeField, HideInInspector] public Transform whiteFish;
    [SerializeField, HideInInspector] public float idleRotateSpeed;
    [SerializeField, HideInInspector] public float sprintRotateSpeed;
    [SerializeField, HideInInspector] public Vector3 Dir;

    [HideInInspector] public SpriteRenderer blackSprite;
    [HideInInspector] public SpriteRenderer whiteSprite;
    [HideInInspector] public Animator blackAnim;
    [HideInInspector] public Animator whiteAnim;

    [FoldoutGroup("Debug", nameof(black_idling), nameof(white_idling),
        nameof(black_sprint_startPoint), nameof(white_sprint_startPoint))]
    public Void void3;

    [SerializeField, HideInInspector] public bool black_idling = true;
    [SerializeField, HideInInspector] public bool white_idling = true;
    [SerializeField, HideInInspector] public bool black_sprint_startPoint = false;
    [SerializeField, HideInInspector] public bool white_sprint_startPoint = false;
    [SerializeField, HideInInspector] public bool black_sprint_back = false;
    [SerializeField, HideInInspector] public bool white_sprint_back = false;

    public bool actions = true;
    [ShowField(nameof(actions)), ButtonField("WaterSpear", "WaterSpear"), SerializeField] private Void void1;
    [SerializeField, ShowField(nameof(actions))] public IEnemyAction waterSpear;

    // Update is called once per frame

    public override void Start()
    {
        base.Start();
        blackSprite = blackFish.GetComponent<SpriteRenderer>();
        whiteSprite = whiteFish.GetComponent<SpriteRenderer>();
        blackAnim = blackFish.GetComponent<Animator>();
        whiteAnim = whiteFish.GetComponent<Animator>();
    }

    private void Update()
    {
        if (black_sprint_startPoint)
        {
            blackAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            blackFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
            if (blackFish.eulerAngles.z <= 5 || blackFish.eulerAngles.z >= 355)// reached start point
            {
                black_sprint_startPoint = false;
                nextAction.Act();
            }
        }
        else if (black_sprint_back)
        {
            blackAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            blackFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            if (angle <= 180 && angle >= 175)// reached start point
            {
                black_sprint_back = false;
                black_idling = true;
            }
        }
        else if (black_idling) { blackFish.RotateAround(transform.position, Dir, idleRotateSpeed * Time.deltaTime); blackAnim.SetFloat("swim_speed", 1f); }

        if (white_sprint_startPoint)
        {
            whiteFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
            whiteAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            if (whiteFish.eulerAngles.z <= 5 || whiteFish.eulerAngles.z >= 355)// reached start point
            {
                white_sprint_startPoint = false;
                nextAction.Act();
            }
        }
        else if (white_sprint_back)
        {
            whiteFish.RotateAround(transform.position, Dir, sprintRotateSpeed * Time.deltaTime);
            whiteAnim.SetFloat("swim_speed", sprintRotateSpeed / idleRotateSpeed);
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            if (angle <= 180 && angle >= 175)// reached start point
            {
                white_sprint_back = false;
                white_idling = true;
            }
        }
        else if (white_idling) { whiteFish.RotateAround(transform.position, Dir, idleRotateSpeed * Time.deltaTime); whiteAnim.SetFloat("swim_speed", 1f); }
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public void WaterSpear()
    {
        print("water spear test");
        nextAction = waterSpear;
        Transform closerFish = CheckCloserFish();
        if (closerFish == blackFish)
        {
            black_idling = false;
            black_sprint_startPoint = true;
        }
        else
        {
            white_idling = false;
            white_sprint_startPoint = true;
        }
    }

    /// <summary>
    /// compare black fish and white fish's X position
    /// </summary>
    /// <returns>the one closer to the start point </returns>
    public Transform CheckCloserFish()
    {
        Transform temp = null;

        //if one of them is busy, return another one
        if (black_idling != white_idling)
        {
            if (black_idling == false) { return whiteFish; }
            else if (white_idling == false) { return blackFish; }
        }
        else// check position.x
        {
            if (black_idling == false) { return null; }
            else if (blackFish.position.x < whiteFish.position.x) { return blackFish; }
            else { return whiteFish; }
        }

        return temp;
    }

    public override int Damage(int damageAmount, Transform sender, float stunDuration = 0)
    {
        if (DEAD) { return 0; }

        flash.OnDamageFlash();
        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;

        if (currentHealth <= 0)
        {
            Invoke("Death", 3f);
            DEAD = true;
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0f;
            rb.isKinematic = true;
            GetComponent<BoxCollider2D>().enabled = false;
            HealthUI.SetActive(false);
            gameObject.layer = 0;
            anim.Play("death");
        }//death

        return 0;
    }

    public override int SubObjectDamage(int damageAmount, Transform sender = null, float stunDuration = 0)
    {
        if (DEAD) { return 0; }

        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;

        if (currentHealth <= 0)
        {
            Invoke("Death", 3f);
            DEAD = true;
            rb.velocity = Vector3.zero;
            rb.gravityScale = 0f;
            rb.isKinematic = true;
            GetComponent<BoxCollider2D>().enabled = false;
            HealthUI.SetActive(false);
            gameObject.layer = 0;
            anim.Play("death");
        }//death

        return 0;
    }
}