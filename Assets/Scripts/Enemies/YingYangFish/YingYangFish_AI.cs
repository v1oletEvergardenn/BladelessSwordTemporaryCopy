using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;

public class YingYangFish_AI : IEnemyController
{
    [FoldoutGroup("Attributes", nameof(center), nameof(blackFish), nameof(blackFishGFX),
        nameof(whiteFish), nameof(whiteFishGFX),
        nameof(idleRotateSpeed), nameof(sprintRotateSpeed), nameof(Dir),
        nameof(swimToCenterSpeed), nameof(minMaxDistanceTocenter))
        ]
    public Void void2;

    [SerializeField, HideInInspector] public Transform center;
    [SerializeField, HideInInspector] public Transform blackFish;
    [SerializeField, HideInInspector] public Transform whiteFish;
    [SerializeField, HideInInspector] public Transform blackFishGFX;
    [SerializeField, HideInInspector] public Transform whiteFishGFX;
    [SerializeField, HideInInspector] public float idleRotateSpeed;
    [SerializeField, HideInInspector] public float sprintRotateSpeed;
    [SerializeField, HideInInspector] public Vector3 Dir;
    [SerializeField, HideInInspector] public float swimToCenterSpeed = 2f;
    [SerializeField, HideInInspector, MinMaxSlider(1f, 3f)] public Vector2 minMaxDistanceTocenter;
    [HideInInspector] public SpriteRenderer blackSprite;
    [HideInInspector] public SpriteRenderer whiteSprite;
    [HideInInspector] public Animator blackAnim;
    [HideInInspector] public Animator whiteAnim;

    [FoldoutGroup("Debug", nameof(black_idling), nameof(white_idling),
        nameof(black_sprint_startPoint), nameof(white_sprint_startPoint), nameof(white_distanceToCenter),
        nameof(black_distanceToCenter))]
    public Void void3;

    [SerializeField, HideInInspector] public bool black_idling = true;
    [SerializeField, HideInInspector] public bool white_idling = true;
    [SerializeField, HideInInspector] public bool black_sprint_startPoint = false;
    [SerializeField, HideInInspector] public bool white_sprint_startPoint = false;
    [SerializeField, HideInInspector] public bool black_sprint_back = false;
    [SerializeField, HideInInspector] public bool white_sprint_back = false;
    [SerializeField, HideInInspector] public float white_distanceToCenter = 0f;
    [SerializeField, HideInInspector] public float black_distanceToCenter = 0f;
    public bool isCloseSwimming;

    public bool actions = true;
    [ShowField(nameof(actions)), ButtonField("CloseSwim", "CloseSwim"), SerializeField] private Void void6;
    [ShowField(nameof(actions)), ButtonField("FarSwim", "FarSwim"), SerializeField] private Void void7;
    [ShowField(nameof(actions)), ButtonField("WaterSpear", "WaterSpear"), SerializeField] private Void void1;
    [ShowField(nameof(actions)), ButtonField("Swing", "Swing"), SerializeField] private Void void8;
    [SerializeField, ShowField(nameof(actions))] public IEnemyAction waterSpear;
    [SerializeField, ShowField(nameof(actions))] public IEnemyAction swing;

    // Update is called once per frame

    public override void Start()
    {
        base.Start();
        blackSprite = blackFish.GetComponent<SpriteRenderer>();
        whiteSprite = whiteFish.GetComponent<SpriteRenderer>();
        blackAnim = blackFishGFX.GetComponent<Animator>();
        whiteAnim = whiteFishGFX.GetComponent<Animator>();
    }

    private void Update()
    {
        white_distanceToCenter = Vector2.Distance(whiteFish.position, center.position);
        black_distanceToCenter = Vector2.Distance(blackFish.position, center.position);

        float tempRotateSpeed = (minMaxDistanceTocenter.y / white_distanceToCenter) * idleRotateSpeed;
        if (tempRotateSpeed >= idleRotateSpeed * 2) { tempRotateSpeed = idleRotateSpeed * 2; }
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
        else if (black_idling) { blackFish.RotateAround(transform.position, Dir, tempRotateSpeed * Time.deltaTime); blackAnim.SetFloat("swim_speed", 1f); }

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
        else if (white_idling) { whiteFish.RotateAround(transform.position, Dir, tempRotateSpeed * Time.deltaTime); whiteAnim.SetFloat("swim_speed", 1f); }
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public void WaterSpear()
    {
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

    public void Swing()
    {
        nextAction = swing;
        if (!isCloseSwimming)
        {
            CloseSwim();
        }
        else
        {
            swing.Act();
        }
    }

    public void CloseSwim()
    {
        StartCoroutine(IECloseSwim(true));
    }

    public void FarSwim()
    {
        StartCoroutine(IECloseSwim(false));
    }

    public IEnumerator IECloseSwim(bool close)
    {
        if (close)
        {
            float elapsedTime = 0f;
            whiteAnim.Play("close_swim_pre");
            blackAnim.Play("close_swim_pre");
            while (white_distanceToCenter > minMaxDistanceTocenter.x)
            {
                whiteFish.position -= whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                blackFish.position -= blackFish.up * swimToCenterSpeed * Time.deltaTime;
                whiteFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, -25), elapsedTime / 0.5f);
                whiteFishGFX.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 0, 0), elapsedTime / 0.5f);
                blackFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, -25), elapsedTime / 0.5f);
                blackFishGFX.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 0, 0), elapsedTime / 0.5f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            nextAction.Act();
        }
        else
        {
            float elapsedTime = 0f;
            float originalRotateSpeed = idleRotateSpeed;
            idleRotateSpeed *= 1.5f;
            whiteAnim.Play("white_idle");
            blackAnim.Play("black_idle");
            while (elapsedTime < 2f)
            {
                if (white_distanceToCenter < minMaxDistanceTocenter.y)
                {
                    whiteFish.position += whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                    blackFish.position += blackFish.up * swimToCenterSpeed * Time.deltaTime;
                }
                whiteFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, -25), new Vector3(0, 0, 0), elapsedTime / 0.5f);
                whiteFishGFX.localPosition = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(0, 0, 0), elapsedTime / 2f);
                blackFishGFX.localEulerAngles = Vector3.Lerp(new Vector3(0, 0, -25), new Vector3(0, 0, 0), elapsedTime / 0.5f);
                blackFishGFX.localPosition = Vector3.Lerp(new Vector3(1, 0, 0), new Vector3(0, 0, 0), elapsedTime / 2f);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            idleRotateSpeed = originalRotateSpeed;
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