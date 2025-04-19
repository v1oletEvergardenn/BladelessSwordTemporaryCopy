using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

[RequireComponent(typeof(DamageFlash), typeof(Rigidbody2D), typeof(IEnemyActionIdle))]
public abstract class IEnemyController : IDamagable
{
    #region HEALTH

    [FoldoutGroup("Health", nameof(maxHealth), nameof(HealthUI), nameof(healthBar))] public Void healthVoid;
    [SerializeField, HideInInspector] public int maxHealth;
    [SerializeField, HideInInspector] public GameObject HealthUI;
    [SerializeField, HideInInspector] public Image healthBar;

    #endregion HEALTH

    #region STUN

    [FoldoutGroup("stunning", nameof(maxStun), nameof(stunBar), nameof(currentStun))] public Void stunVoid1;
    [SerializeField, HideInInspector] public int maxStun = 10;
    [SerializeField, HideInInspector] public Image stunBar;
    [SerializeField, HideInInspector] public int currentStun;

    #endregion STUN

    #region GROUNDCHECK

    [FoldoutGroup("GroundCheck", nameof(isGrounded), nameof(m_WhatIsGround), nameof(col))] public Void groundvoid;
    [SerializeField, HideInInspector] public bool isGrounded = true;
    [SerializeField, HideInInspector] public LayerMask m_WhatIsGround;
    [SerializeField, HideInInspector] public Collider2D col;

    #endregion GROUNDCHECK

    #region BASIC_LOGIC

    [FoldoutGroup("basic_logic", nameof(IN_COMBAT), nameof(canFlip), nameof(isFacingRight),
        nameof(inAct), nameof(speed), nameof(leftBoundary), nameof(rightBoundary), nameof(GFX),
        nameof(maxActionBreakCapacity), nameof(currentActionBreakAmount), nameof(breakDuration))]
    public Void logicvoid;

    [SerializeField, HideInInspector] public bool IN_COMBAT = false;
    [SerializeField, HideInInspector] public bool canFlip = true;
    [SerializeField, HideInInspector] public bool isFacingRight;
    [SerializeField, HideInInspector] public bool inAct = false;
    [SerializeField, HideInInspector] public float speed = 20f;
    [SerializeField, HideInInspector] public Transform leftBoundary;
    [SerializeField, HideInInspector] public Transform rightBoundary;
    [SerializeField, HideInInspector] public GameObject GFX;
    [SerializeField, HideInInspector] public int maxActionBreakCapacity = 10;
    [SerializeField, HideInInspector] public int currentActionBreakAmount = 0;
    [SerializeField, HideInInspector] public float breakDuration = 3f;

    [HideInInspector] public List<IEnemyAction> actionList = new List<IEnemyAction>();

    #endregion BASIC_LOGIC

    #region PRIVATE VARIABLES

    [HideInInspector] public bool isActing = false;
    [HideInInspector] public IEnemyAction initialAction;
    [HideInInspector] public bool canMove = false;
    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool DEAD = false;
    [HideInInspector] public DamageFlash flash;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public AnimationCurve outline_flash_anim_curve;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public Animator anim;
    [HideInInspector] public Transform player;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public Energy playerEnergy;
    [HideInInspector] public CharacterController2D playerController;

    #endregion PRIVATE VARIABLES

    public virtual void Start()
    {
        sprite = GFX.GetComponent<SpriteRenderer>();
        playerAttack = PlayerAttack.instance;
        playerEnergy = Energy.instance;
        playerController = CharacterController2D.instance;
        player = playerAttack.gameObject.transform;
        anim = GFX.GetComponent<Animator>();
        currentHealth = maxHealth;
        healthBar.fillAmount = currentHealth / maxHealth;
        stunBar.fillAmount = currentStun / maxStun;
        flash = GetComponent<DamageFlash>();
        rb = GetComponent<Rigidbody2D>();
        outline_flash_anim_curve = GameManager.instance.outline_flash_anim_curve;
        actionList.Clear();
    }

    public virtual IEnemyAction NextAction()
    {
        if (actionList.Count < 2) { return null; }
        return actionList[1];
    }

    public virtual void EndAction()
    {
        if (actionList.Count > 0)
        {
            actionList.RemoveAt(0);
        }
    }

    public virtual void InsertAction(IEnemyAction action, int index = 1)
    {
        if (actionList.Count == 0) { actionList.Add(action); }
        else
        {
            actionList.Insert(index, action);
        }
    }

    public Coroutine co_act;

    public virtual IEnumerator Act()
    {
        while (actionList.Count > 0 && actionList[0] != null)
        {
            IEnemyAction action = actionList[0];
            yield return action.act_routine = StartCoroutine(action.Act_coroutine());
            yield return null;
        }
        isActing = false;
        if (currentActionBreakAmount >= maxActionBreakCapacity) { yield return StartCoroutine(Break()); }//break
        else { StartAction(); }//startover
        yield return null;
    }

    public virtual IEnumerator Break()
    {
        yield return new WaitForSeconds(breakDuration);
        currentActionBreakAmount = 0;
        StartAction();
        yield return null;
    }

    public virtual void AddActionBreak(int amount)
    {
        currentActionBreakAmount += amount;
    }

    public virtual void OutLine_Activate(int i)
    {
        sprite.material.SetFloat("_OutLine", i);
    }

    public virtual void OutLineFlash()
    {
        StartCoroutine(IEOutLineFlash());
    }

    public virtual IEnumerator IEOutLineFlash()
    {
        float currentFlashAmount = 0f;
        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = outline_flash_anim_curve.Evaluate(elapsedTime);
            sprite.material.SetFloat("_OutLineThickness", currentFlashAmount);
            yield return null;
        }
    }

    public virtual void ActivateCombat()
    {
        StartCoroutine(IE_Activate());
    }

    public virtual IEnumerator IE_Activate()
    {
        IN_COMBAT = true;
        yield return null;
    }

    public virtual void StartAction()
    {
    }

    public virtual void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;
        RaycastHit2D ray = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + 0.1f, m_WhatIsGround);
        if (ray.collider != null)
        {
            isGrounded = true;
        }
    }

    public bool Possibility(float i)
    {
        float chance = UnityEngine.Random.Range(0, 100);
        if (chance <= i) { return true; }
        else { return false; }
    }

    public virtual Transform GetCloseBoundary()
    {
        float mid = (leftBoundary.position.x + rightBoundary.position.x) / 2;
        if (transform.position.x < mid) { return leftBoundary; }
        else { return rightBoundary; }
    }

    public virtual Transform GetFarBoundary()
    {
        float mid = (leftBoundary.position.x + rightBoundary.position.x) / 2;
        if (transform.position.x < mid) { return rightBoundary; }
        else { return leftBoundary; }
    }

    public virtual Transform GetBoundaryFarOfPlayer()
    {
        float mid = (leftBoundary.position.x + rightBoundary.position.x) / 2;
        if (player.position.x < mid) { return rightBoundary; }
        else { return leftBoundary; }
    }

    public virtual void ForceDie()
    {
        Damage(maxHealth);
    }
}