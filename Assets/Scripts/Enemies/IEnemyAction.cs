using System.Collections;
using UnityEngine;
using EditorAttributes;

public abstract class IEnemyAction : MonoBehaviour
{
    [HideInInspector] public Animator anim;
    [HideInInspector] public IEnemyController controller;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public GameObject player;
    [HideInInspector] public Health playerIDamagable;
    [HideInInspector] public ObjectPooler pooler;
    [HideInInspector] public VFXManager vfx;
    [HideInInspector] public Rigidbody2D rb;
    public float action_time = 1f;
    public bool isThisActing = false;

    public virtual void Start()
    {
        playerIDamagable = Health.instance;
        player = playerIDamagable.gameObject;
        vfx = VFXManager.instance;
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<IEnemyController>();
        anim = controller.anim;
        sprite = controller.sprite;
        pooler = ObjectPooler.instance;
    }

    public virtual void Act()
    {
        StartCoroutine(Act_coroutine());
    }

    public virtual void CancelAct()
    {
        StopAllCoroutines();
        isThisActing = false;
        //OutLine_Activate(0);
    }

    public virtual IEnumerator Act_coroutine()
    {
        yield return null;
    }
}