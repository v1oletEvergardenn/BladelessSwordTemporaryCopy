using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using Unity.VisualScripting;
using JetBrains.Annotations;
using DG.Tweening;

public class YingYangFish_AI : IEnemyController
{
    #region ATTRIBUTES

    [FoldoutGroup("Attributes", nameof(center), nameof(blackFish), nameof(blackFishGFX),
        nameof(whiteFish), nameof(whiteFishGFX), nameof(blackOrigin), nameof(whiteOrigin),
        nameof(idleRotateSpeed), nameof(sprintRotateSpeed), nameof(Dir),
        nameof(swimToCenterSpeed), nameof(minMaxDistanceTocenter), nameof(waterLevel),
        nameof(EventInteract))
        ]
    public Void void2;

    [SerializeField, HideInInspector] public Transform YingYangFish;
    [SerializeField, HideInInspector] public Transform center;
    [SerializeField, HideInInspector] public Transform blackFish;
    [SerializeField, HideInInspector] public Transform whiteFish;
    [SerializeField, HideInInspector] public Transform blackFishGFX;
    [SerializeField, HideInInspector] public Transform whiteFishGFX;
    [SerializeField, HideInInspector] public Transform blackOrigin;
    [SerializeField, HideInInspector] public Transform whiteOrigin;
    [SerializeField, HideInInspector] public float idleRotateSpeed;
    [SerializeField, HideInInspector] public float sprintRotateSpeed;
    [HideInInspector] public float black_rotateSpeed;
    [HideInInspector] public float white_rotateSpeed;
    [HideInInspector] public float black_targetRotateSpeed;
    [HideInInspector] public float white_targetRotateSpeed;
    [SerializeField, HideInInspector] public Vector3 Dir;
    [SerializeField, HideInInspector] public float swimToCenterSpeed = 2f;
    [SerializeField, HideInInspector, MinMaxSlider(1f, 3f)] public Vector2 minMaxDistanceTocenter;
    [SerializeField, HideInInspector] public Transform waterLevel;
    [SerializeField, HideInInspector] public GameObject EventInteract;

    [HideInInspector] public SpriteRenderer blackSprite;
    [HideInInspector] public SpriteRenderer whiteSprite;
    [HideInInspector] public Animator blackAnim;
    [HideInInspector] public Animator whiteAnim;
    [HideInInspector] public Animator centerAnim;
    [HideInInspector] public bool closerFish_Black;

    #endregion ATTRIBUTES

    #region DEBUG

    [FoldoutGroup("Debug", nameof(white_distanceToCenter),
        nameof(black_distanceToCenter), nameof(movingTarget), nameof(isCloseSwimming), nameof(secondPhase))]
    public Void void3;

    //[SerializeField, HideInInspector] public bool black_idling = true;
    //[SerializeField, HideInInspector] public bool white_idling = true;
    [SerializeField, HideInInspector] public float white_distanceToCenter = 0f;

    [SerializeField, HideInInspector] public float black_distanceToCenter = 0f;
    [SerializeField, HideInInspector] public Transform movingTarget;
    [SerializeField, HideInInspector] public bool isCloseSwimming;
    [SerializeField, HideInInspector] public bool secondPhase;

    #endregion DEBUG

    #region ACTIONS

    public bool Actions;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("ForceDie", "ForceDie")] public Transform void112;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("StartAction", "StartAction")] public Transform void11;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("WaterSpear", "WaterSpear")] public Void void1;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Swing", "Swing")] public Void void8;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Splash_white", "Splash_white")] public Void void9;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Splash_black", "Splash_black")] public Void void13;
    [ShowField(nameof(Actions))][SerializeField, ButtonField("Dive", "Dive")] public Void void10;
    [ShowField(nameof(Actions))][SerializeField] public YYF_WaterSpear waterSpear;
    [ShowField(nameof(Actions))][SerializeField] public YYF_Swing swing;
    [ShowField(nameof(Actions))][SerializeField] public YYF_splash_white splash_white;
    [ShowField(nameof(Actions))][SerializeField] public YYF_splash_black splash_black;
    [ShowField(nameof(Actions))][SerializeField] public YYF_Dive dive;

    #endregion ACTIONS

    [HideInInspector] public Coroutine co_sprintStartPoint;
    [HideInInspector] public Coroutine co_IEcloseSwim;
    [HideInInspector] public Coroutine co_sprintBackEqual;

    // Update is called once per frame

    public override void Start()
    {
        base.Start();
        blackSprite = blackFish.GetComponent<SpriteRenderer>();
        whiteSprite = whiteFish.GetComponent<SpriteRenderer>();
        blackAnim = blackFishGFX.GetComponent<Animator>();
        whiteAnim = whiteFishGFX.GetComponent<Animator>();
        centerAnim = center.GetComponent<Animator>();
        movingTarget = player;
        EventInteract.SetActive(true);
        HealthUI.SetActive(false);

        white_rotateSpeed = idleRotateSpeed;
        white_targetRotateSpeed = idleRotateSpeed;
        black_rotateSpeed = idleRotateSpeed;
        black_targetRotateSpeed = idleRotateSpeed;
    }

    private float white_rotateVelocity;
    private float black_rotateVelocity;

    private void Update()
    {
        distanceToPlayer = Mathf.Abs(transform.position.x - player.position.x);
        white_distanceToCenter = Vector2.Distance(whiteFish.position, center.position);
        black_distanceToCenter = Vector2.Distance(blackFish.position, center.position);

        float white_temp = (minMaxDistanceTocenter.y / white_distanceToCenter) * white_targetRotateSpeed;
        float black_temp = (minMaxDistanceTocenter.y / black_distanceToCenter) * black_targetRotateSpeed;

        white_rotateSpeed = Mathf.SmoothDamp(white_rotateSpeed, white_temp, ref white_rotateVelocity, 0.2f);
        black_rotateSpeed = Mathf.SmoothDamp(black_rotateSpeed, black_temp, ref black_rotateVelocity, 0.2f);

        blackOrigin.Rotate(new Vector3(0, 0, -1), black_rotateSpeed * Time.deltaTime);
        whiteOrigin.Rotate(new Vector3(0, 0, -1), white_rotateSpeed * Time.deltaTime);

        if (!isActing && actionList.Count > 0)
        {
            isActing = true;
            co_act = StartCoroutine(Act());
        }
    }

    public override IEnumerator IE_Activate()
    {
        IN_COMBAT = true;
        //play start animation
        co_sprintBackEqual = StartCoroutine(SprintBackEqual());
        yield return co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        HealthUI.SetActive(true);
        StartAction();
        //start action loops
    }

    public override IEnumerator Act()
    {
        while (actionList.Count > 0 && actionList[0] != null)
        {
            IEnemyAction action = actionList[0];
            yield return action.act_routine = StartCoroutine(action.Act_coroutine());
            yield return null;
        }
        isActing = false;
        if (!secondPhase && currentActionBreakAmount >= maxActionBreakCapacity) { yield return StartCoroutine(Break()); }//break
        else { StartAction(); }//startover
        yield return null;
    }

    public override void StartAction()
    {
        if (secondPhase) { return; }
        List<IEnemyAction> possibleActions = new List<IEnemyAction>();
        if (playerEnergy.currentEnergy <= 5)
        {
            possibleActions.Add(waterSpear);
        }
        if (Mathf.Abs(player.position.x - transform.position.x) >= swing.swingRange - 1)
        {
            float i = Random.Range(0, 10);
            if (i < 3) { possibleActions.Add(waterSpear); }
            else if (i < 6) { possibleActions.Add(splash_white); }
            else if (i < 10) { possibleActions.Add(dive); }//moving
        }
        else
        {
            possibleActions.Add(swing);
        }

        if (!playerController.isGrounded)
        {
            possibleActions.Add(splash_black);
        }

        int index = Random.Range(0, possibleActions.Count);
        initialAction = possibleActions[index];

        actionList.Add(initialAction);
        if (initialAction == dive)
        {
            if ((float)playerEnergy.currentEnergy / (float)playerEnergy.maxEnergy <= 0.7f || playerAttack.currentHS_point < 2)
            {
                InsertAction(swing);
                movingTarget = player;
                print("here");
            }
            else
            {
                movingTarget = GetBoundaryFarOfPlayer();
                if (Possibility(50)) { InsertAction(waterSpear); }
                else { InsertAction(splash_white); InsertAction(waterSpear); }
            }
        }
    }

    /// <summary>
    /// designated fish runs faster to get to start point(top) for next action
    /// </summary>
    /// <param name="isBlack"></param>
    public IEnumerator SprintStartPoint()
    {
        Transform closerFish = CheckCloserFish();
        bool finished = false;
        if (closerFish == blackFish)
        {
            closerFish_Black = true;
            black_targetRotateSpeed = 0;
            blackAnim.Play("sprint");
            blackOrigin.DORotate(Vector3.zero, sprintRotateSpeed, RotateMode.FastBeyond360).SetSpeedBased(true).OnComplete(() => { finished = true; });
        }
        else
        {
            closerFish_Black = false;
            white_targetRotateSpeed = 0;
            whiteAnim.Play("sprint");
            whiteOrigin.DORotate(Vector3.zero, sprintRotateSpeed, RotateMode.FastBeyond360).SetSpeedBased(true).OnComplete(() => { finished = true; });
        }
        while (!finished) { yield return null; }
        yield return null;
    }

    /// <summary>
    /// designated fish runs faster to get back to equal position
    /// </summary>
    /// <param name="isBlack"></param>
    public IEnumerator SprintBackEqual()
    {
        float closerFish = Vector2.SignedAngle(blackFish.right, whiteFish.right);

        if (closerFish > 0 && closerFish <= 180)
        {
            black_targetRotateSpeed = sprintRotateSpeed;
            if (white_targetRotateSpeed == sprintRotateSpeed) { black_targetRotateSpeed = white_targetRotateSpeed * 2; }

            blackAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 165)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right); yield return null;
            }
            black_targetRotateSpeed = idleRotateSpeed;
        }
        else
        {
            white_targetRotateSpeed = sprintRotateSpeed;
            if (black_targetRotateSpeed == sprintRotateSpeed) { white_targetRotateSpeed = black_targetRotateSpeed * 2; }
            whiteAnim.Play("sprint");
            float angle = Vector2.Angle(blackFish.right, whiteFish.right);
            while (angle > 180 || angle < 165)// reached start point
            {
                angle = Vector2.Angle(blackFish.right, whiteFish.right); yield return null;
            }
            white_targetRotateSpeed = idleRotateSpeed;
        }
        yield return null;
    }

    public IEnumerator IECloseSwim(bool close, bool playAnim = true)
    {
        white_targetRotateSpeed = sprintRotateSpeed;
        black_targetRotateSpeed = sprintRotateSpeed;

        if (close)
        {
            if (playAnim)
            {
                whiteAnim.Play("close_swim_pre");
                blackAnim.Play("close_swim_pre");
            }

            while (white_distanceToCenter > minMaxDistanceTocenter.x)
            {
                whiteFish.position -= whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                blackFish.position -= blackFish.up * swimToCenterSpeed * Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            blackAnim.Play("sprint"); whiteAnim.Play("sprint");

            while (white_distanceToCenter < minMaxDistanceTocenter.y)
            {
                whiteFish.position += whiteFish.up * swimToCenterSpeed * Time.deltaTime;
                blackFish.position += blackFish.up * swimToCenterSpeed * Time.deltaTime;
                yield return null;
            }
        }
    }

    /// <summary>
    /// compare black fish and white fish's X position
    /// </summary>
    /// <returns>the one closer to the start point </returns>
    public Transform CheckCloserFish()
    {
        if (blackFish.eulerAngles.z < whiteFish.eulerAngles.z) { return blackFish; }
        else { return whiteFish; }
    }

    public override int Damage(int damageAmount, Transform sender, float stunDuration = 0)
    {
        if (DEAD) { return 0; }

        flash.OnDamageFlash();
        currentHealth -= damageAmount;
        healthBar.fillAmount = (float)currentHealth / (float)maxHealth;

        if (currentHealth <= 0)
        {
            StartCoroutine(Pre_SecondPhase());
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
            //Invoke("Death", 3f);
            //DEAD = true;
            //rb.velocity = Vector3.zero;
            //rb.gravityScale = 0f;
            //rb.isKinematic = true;
            //GetComponent<BoxCollider2D>().enabled = false;
            //HealthUI.SetActive(false);
            //gameObject.layer = 0;
            //anim.Play("death");
            StartCoroutine(Pre_SecondPhase());
        }//death

        return 0;
    }

    public Quaternion CalculateWantedRotation(Vector3 _targetPos)
    {
        float angle = Mathf.Atan2(_targetPos.y - transform.position.y, _targetPos.x - transform.position.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        return targetRotation;
    }

    public void CancelAllAction()
    {
        if (co_IEcloseSwim != null) StopCoroutine(co_IEcloseSwim);
        if (co_sprintBackEqual != null) StopCoroutine(co_sprintBackEqual);
        if (co_sprintStartPoint != null) StopCoroutine(co_sprintStartPoint);

        StopCoroutine(co_act);
        waterSpear.CancelAct();
        swing.CancelAct();
        splash_white.CancelAct();
        splash_black.CancelAct();
        dive.CancelAct();
        SetNormalRotateSpeed();
    }

    public void Interact()
    {
        //play start animation
        //start fight
        if (!secondPhase)
        {
            StartCoroutine(IE_Activate());
            EventInteract.SetActive(false);
        }
        else
        {
            //start second phase
            StartCoroutine(secondPhaseAnim());
        }
    }

    public IEnumerator Pre_SecondPhase()
    {
        if (!secondPhase)
        {
            actionList.Clear();
            CancelAllAction();
            co_IEcloseSwim = StartCoroutine(IECloseSwim(true));

            CharacterUIManager.ShowBlackEdge(true);
            yield return co_sprintBackEqual = StartCoroutine(SprintBackEqual());
            white_targetRotateSpeed = idleRotateSpeed / 3;
            black_targetRotateSpeed = idleRotateSpeed / 3;
            yield return StartCoroutine(ChangeYPos(false));

            yield return new WaitForSeconds(1.5f);
            centerAnim.Play("center_break");
            SoundManager.PlaySound("glass_break");
            secondPhase = true;
            EventInteract.SetActive(true);
        }
    }

    public IEnumerator secondPhaseAnim()
    {
        EventInteract.SetActive(false);
        centerAnim.Play("center_fade");
        yield return new WaitForSeconds(1.6f);
        co_IEcloseSwim = StartCoroutine(IECloseSwim(false));
        StartCoroutine(ChangeYPos(true));
        StartCoroutine(Ultimate());
        yield return null;
    }

    public IEnumerator Ultimate()
    {
        //shaking and rotating
        //normal rotating
        // dive and QTE
        // up on right
        // splash four times
        yield return true;
    }

    public IEnumerator ChangeYPos(bool up)
    {
        float y_value = waterLevel.position.y + 4.5f;
        if (!up) { y_value = waterLevel.position.y + 2f; }

        if (transform.position.y < y_value)
        {
            while (transform.position.y < y_value) { transform.position += new Vector3(0, 1, 0) * Time.deltaTime * 3; yield return null; }
        }
        else
        {
            while (transform.position.y > y_value) { transform.position -= new Vector3(0, 1, 0) * Time.deltaTime * 3; yield return null; }
        }
    }

    public void WaterSpear()
    {
        InsertAction(waterSpear);
    }

    public override void ForceDie()
    {
        base.ForceDie();
    }

    public void Swing()
    {
        InsertAction(swing);
    }

    public void Splash_black()
    {
        InsertAction(splash_black);
    }

    public void Splash_white()
    {
        InsertAction(splash_white);
    }

    public void Dive()
    {
        InsertAction(dive);
    }

    public void SetNormalRotateSpeed()
    {
        black_targetRotateSpeed = idleRotateSpeed;
        white_targetRotateSpeed = idleRotateSpeed;
    }

    public void StopRotate()
    {
        black_targetRotateSpeed = 0;
        white_targetRotateSpeed = 0;
    }
}